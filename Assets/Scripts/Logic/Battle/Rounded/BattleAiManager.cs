using UnityEngine;
using Model;
using System.Collections.Generic;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;

public class BattleAIManager : Singleton<BattleAIManager>
{
    public BattleCharacter AiCharacter { get; private set; }

    private Dictionary<KeyValuePair<Skill, Vector3Int>, AISelectResult> selectCache = new ();

    public void BindCharacter(BattleCharacter character)
    {
        AiCharacter = character;
    }
    public async UniTask<BattleAIResult> GetAIResult(BattleCharacter character)
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

    public async UniTask WaitForTurnComplete()
    {
        if (AiCharacter == null) return;
        BattleAIResult result = await GetAIResult(AiCharacter);
        while (result.action != null)
        {
            await result.DoAIAction();
            result = await GetAIResult(AiCharacter);
        }
    }

    private async UniTask<AISelectResult> GetAISelectResult(BattleSkillAction skillAction, Vector3Int position, bool ignoreMove = false)
    {
        KeyValuePair<Skill, Vector3Int> cacheKey = new KeyValuePair<Skill, Vector3Int>(skillAction.skill, position);
    
        if (selectCache.ContainsKey(cacheKey)) return selectCache[cacheKey];

        skillAction.selectRange = BattleBlockManager.Instance.GetBlocksByRange(position, skillAction.skill.SelectRange, BlockRangeType.Circle);

        if (skillAction.selectRange.Count <= 0)
        {
            selectCache.Add(cacheKey, default);
            return default;
        }


        var selectableBlocks = GetAllSelectableBlock(skillAction);

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

            bool castable = skillAction.isExecutable() && (skillAction.battleCostResult == null || skillAction.battleCostResult.CouldCost());

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
                await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
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
        return new BattlePositionBlock(originPos).GetDistance(new BattlePositionBlock(targetPos));
    }

    private async UniTask<int> GetBattleMoveScore(BattleSkillAction moveAction, BattleEffectResult result)
    {
        BattleMoveEffect moveEffect = result.effects.Find(effect => effect is BattleMoveEffect) as BattleMoveEffect;
        Vector3Int originPos = moveAction.position;
        Vector3Int targetPos = moveEffect.position;

        const int penalty = -2;
        int score = penalty; // fixed penalty for move
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

        if (score <= 0) // no extra score after move 
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
        var enemies = BattleManager.Instance.TurnManager.AllRoles.Gen().Where(role => role.isAlive && role.TeamID != AiCharacter.TeamID);

        int minDist = int.MaxValue;
        foreach (var enemy in enemies)
        {
            int distance = new BattlePositionBlock(enemy.Position).GetDistance(new BattlePositionBlock(position));
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

        return attackBlocks.Gen().Count(block =>
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

    private IEnumerable<BattlePositionBlock> GetAllSelectableBlock(BattleSkillAction action)
    {
        return action.selectRange.Gen().Where(block =>
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
        }).AsEnumerable();
    }
}

public struct AISelectResult
{
    public BattlePositionBlock block;
    public BattleCharacter target;
    public BattleEffectResult effectResult;
    public int score;
}

public class BattleAIResult
{
    public BattleSkillAction action;
    public AISelectResult aISelect;

    public async UniTask DoAIAction()
    {
        if (action == null) return;
        var couldCast = action.isExecutable() && (action.battleCostResult == null || action.battleCostResult.CouldCost());
        if (couldCast)
        {
            Debug.Log($"AI Action {action.skill.ID} {action.selectResult.Position}");
            action.battleCostResult?.Cost();
            if (aISelect.effectResult != null)
            {
                await aISelect.effectResult.DoEffect();
            }
            action.self.skills.Remove(action.skill);
            BattleLogConsole.Instance.LogBattleEffect(action, aISelect.effectResult);
        }
        await YieldCoroutine.WaitForSeconds(0.05f);
    }
}