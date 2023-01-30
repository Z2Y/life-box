using System;
using UnityEngine;
using Model;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BattleAIManager : Singleton<BattleAIManager>
{
    public BattleCharacter AiCharacter { get; private set; }

    private Dictionary<KeyValuePair<Skill, Vector3Int>, AISelectResult> selectCache = new Dictionary<KeyValuePair<Skill, Vector3Int>, AISelectResult>();

    public void BindCharacter(BattleCharacter character)
    {
        AiCharacter = character;
    }
    public async Task<BattleAIResult> GetAIResult(BattleCharacter character)
    {
        BattleAIResult result = new BattleAIResult();
        int maxScore = -1;

        selectCache.Clear();
        foreach (var skill in character.skills)
        {
            BattleSkillAction action = new BattleSkillAction();
            action.self = character;
            action.skill = skill;

            AISelectResult aiSelect = await GetAISelectResult(action, character.Position);

            if (aiSelect.block == null) continue;
            if (aiSelect.score > maxScore)
            {
                maxScore = aiSelect.score;
                action.selectResult = aiSelect.block;
                action.target = aiSelect.target;
                result.action = action;
                result.aISelect = aiSelect;
            }
        }

        return result;
    }

    public async Task WaitForTurnComplete()
    {
        if (AiCharacter == null) return;
        BattleAIResult result = await GetAIResult(AiCharacter);
        while (result.action != null)
        {
            await result.DoAIAction();
            result = await GetAIResult(AiCharacter);
        }
    }

    private async Task<AISelectResult> GetAISelectResult(BattleSkillAction skillAction, Vector3Int position, bool ignoreMove = false)
    {
        KeyValuePair<Skill, Vector3Int> cacheKey = new KeyValuePair<Skill, Vector3Int>(skillAction.skill, position);
    
        if (selectCache.ContainsKey(cacheKey)) return selectCache[cacheKey];

        skillAction.selectRange = BattleBlockManager.Instance.GetBlocksByRange(position, skillAction.skill.SelectRange, BlockRangeType.Circle);

        if (skillAction.selectRange.Count <= 0)
        {
            selectCache.Add(cacheKey, default);
            return default;
        }


        List<BattlePositonBlock> selectableBlocks = GetAllSelectableBlock(skillAction);

        int frameCount = 0;
        int maxScore = 0;
        AISelectResult aiSelect = default;
        var env = new Dictionary<string, object>() { { "Skill", skillAction } };

        skillAction.battleCostResult = await skillAction.skill.Cost.ExecuteExpressionAsync(env) as BattleCostResult;

        foreach (var block in selectableBlocks)
        {
            
            skillAction.selectResult = block;
            skillAction.position = position;
            skillAction.target = GetSelectTarget(skillAction);
            var skillEffect = await skillAction.skill.Effect.ExecuteExpressionAsync(env) as BattleEffectResult;

            bool castable = skillAction.isExecuteable() && (skillAction.battleCostResult == null || skillAction.battleCostResult.CouldCost());

            if (castable && skillEffect != null)
            {
                int moveScore = 0;
                bool isMove = HasBattleMoveEffect(skillEffect);
                if (!ignoreMove && isMove)
                {
                    moveScore = await GetBattleMoveScore(skillAction, skillEffect);
                }
                int effectScore = skillEffect.GetScore() + moveScore;
                if (effectScore > maxScore)
                {
                    maxScore = effectScore;
                    aiSelect.block = block;
                    aiSelect.score = effectScore;
                    aiSelect.target = skillAction.target;
                    aiSelect.effectResult = skillEffect;
                }
            }
            frameCount++;
            if (frameCount > 5)
            {
                frameCount = 0;
                await YieldCoroutine.Instance.WaitForInstruction(new WaitForEndOfFrame());
            }
        }
        selectCache.Add(cacheKey, aiSelect);
        return aiSelect;
    }

    private BattleCharacter GetSelectTarget(BattleSkillAction skillAction)
    {
        if (skillAction.skill.SelectType == SelectType.EmptyBlock) return null;
        var character = BattleManager.Instance.TurnManager.GetCharacterByPosition(skillAction.selectResult.Position);
        if (character == null) return null;
        if (skillAction.skill.SelectType == SelectType.EnemyBlock)
        {
            return character.TeamID != skillAction.self.TeamID ? character : null;
        }
        else if (skillAction.skill.SelectType == SelectType.FriendBlock)
        {
            return character.TeamID == skillAction.self.TeamID ? character : null;
        }
        else
        {
            return character;
        }
    }

    private bool HasBattleMoveEffect(BattleEffectResult result)
    {
        return result.effects.Exists((effect) => effect is BattleMoveEffect);
    }

    private int GetMoveDistance(BattleSkillAction moveAction, BattleEffectResult result)
    {
        if (result == null) return 0;
        BattleMoveEffect moveEffect = result.effects.Find(effect => effect is BattleMoveEffect) as BattleMoveEffect;
        Vector3Int originPos = moveAction.position;
        Vector3Int targetPos = moveEffect.position;
        return new BattlePositonBlock(originPos).GetDistance(new BattlePositonBlock(targetPos));
    }

    private async Task<int> GetBattleMoveScore(BattleSkillAction moveAction, BattleEffectResult result)
    {
        BattleMoveEffect moveEffect = result.effects.Find(effect => effect is BattleMoveEffect) as BattleMoveEffect;
        Vector3Int originPos = moveAction.position;
        Vector3Int targetPos = moveEffect.position;

        int penality = -2;
        int score = penality; // fixed penality for move
        int minSelectRange = int.MaxValue;

        foreach (var skill in AiCharacter.skills)
        {
            if (skill == moveAction.skill) continue;
            BattleSkillAction action = new BattleSkillAction();
            action.self = AiCharacter;
            action.skill = skill;
            if (skill.SelectRange < minSelectRange)
            {
                minSelectRange = skill.SelectRange;
            }

            AISelectResult beforeMoveResult = await GetAISelectResult(action, originPos, true);

            AISelectResult afterMoveResult = await GetAISelectResult(action, targetPos, true);

            int diff = afterMoveResult.score - beforeMoveResult.score;

            score += diff;
        }

        if (score <= 0) // no addtional score after move 
        {
            int beforeMoveNearestEnemyDis = GetNearestEnemyDistance(originPos);
            int afterMoveNearestEnemyDis = GetNearestEnemyDistance(targetPos);

            bool beforeInMinSelectRange = minSelectRange >= beforeMoveNearestEnemyDis;
            bool afterInMinSelectRange = minSelectRange >= afterMoveNearestEnemyDis;
            if (!beforeInMinSelectRange || !afterInMinSelectRange)
            {
                score += beforeMoveNearestEnemyDis - afterMoveNearestEnemyDis; // 距离敌人越近 分数越高
            }
        }

        return score;
    }

    private int GetNearestEnemyDistance(Vector3Int position)
    {
        List<BattleCharacter> enemies = BattleManager.Instance.TurnManager.AllRoles.Where(role => role.isAlive && role.TeamID != AiCharacter.TeamID).ToList();

        int minDist = int.MaxValue;
        foreach (var enemy in enemies)
        {
            int distance = new BattlePositonBlock(enemy.Position).GetDistance(new BattlePositonBlock(position));
            if (distance < minDist)
            {
                minDist = distance;
            }
        }
        return minDist;
    }

    private int GetTargetCountInRange(BattleSkillAction skillAction)
    {
        if (skillAction.skill.SkillType == SkillType.Move) return 0;
        int attackRange = skillAction.skill.AttackRange;
        var attackBlocks = BattleBlockManager.Instance.GetBlocksByRange(skillAction.selectResult.Position, attackRange, BlockRangeType.Circle);

        if (attackRange > 0)
        {
            attackBlocks.Add(skillAction.selectResult);
        }

        return attackBlocks.Count(block =>
        {
            var character = BattleManager.Instance.TurnManager.GetCharacterByPosition(block.Position);
            if (skillAction.skill.SkillType == SkillType.Attack)
            {
                return character != null && character.TeamID != skillAction.self.TeamID;
            }
            else
            {
                return character != null && character.TeamID == skillAction.self.TeamID;
            }
        });
    }

    private List<BattlePositonBlock> GetAllSelectableBlock(BattleSkillAction action)
    {
        return action.selectRange.Where(block =>
        {
            BattleCharacter blockCharacter = BattleManager.Instance.TurnManager.GetCharacterByPosition(block.Position);
            bool isEnemy = blockCharacter != null && blockCharacter.TeamID != action.self.TeamID;
            bool isFriend = blockCharacter != null && blockCharacter.TeamID == action.self.TeamID;
            bool isEmpty = blockCharacter == null;
            switch (action.skill.SelectType)
            {
                case SelectType.EmptyBlock:
                    return isEmpty;
                case SelectType.EnemyBlock:
                    return isEnemy;
                case SelectType.FriendBlock:
                    return isFriend;
                case SelectType.AnyBlock:
                default:
                    return true;
            }
        }).ToList();
    }
}

public struct AISelectResult
{
    public BattlePositonBlock block;
    public BattleCharacter target;
    public BattleEffectResult effectResult;
    public int score;
}

public class BattleAIResult
{
    public BattleSkillAction action;
    public AISelectResult aISelect;

    public async Task DoAIAction()
    {
        if (action == null) return;
        bool castable = action.isExecuteable() && (action.battleCostResult == null || action.battleCostResult.CouldCost());
        if (castable)
        {
            UnityEngine.Debug.Log($"AI Action {action.skill.ID} {action.selectResult.Position}");
            action.battleCostResult?.Cost();
            await aISelect.effectResult?.DoEffect();
            action.self.skills.Remove(action.skill);
            BattleLogConsole.Instance.LogBattleEffect(action, aISelect.effectResult);
        }
        await YieldCoroutine.Instance.WaitForInstruction(new WaitForSeconds(0.05f));
    }
}