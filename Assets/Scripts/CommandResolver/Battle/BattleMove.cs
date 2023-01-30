using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;

[CommandResolverHandler("BattleMove")]
public class BattleMove : CommandResolver
{
    public TaskCompletionSource<bool> moveTcs;
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        BattleSkillAction skillAction = env["Skill"] as BattleSkillAction;
        if (skillAction == null) return null;

        BattleMoveEffect result = new BattleMoveEffect(skillAction.self, skillAction.selectResult.Position);

        await this.Done();
        return result;
    }
}

[CommandResolverHandler("BattleEffect")]
public class BattleEffectResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        BattleEffectResult result = new BattleEffectResult();
        result.effects = env.Values.Where(value => value is IBattleEffect).Select(value => value as IBattleEffect).ToList();
        await this.Done();
        return result;
    }
}

public interface IBattleEffect
{
    Task DoEffect();
    string EffectDescription();
    int GetScore();
}

public class BattleEffectResult
{
    public List<IBattleEffect> effects = new List<IBattleEffect>();

    public async Task DoEffect()
    {
        foreach(var effect in effects) {
            await effect.DoEffect();
        }
    }

    public int GetScore()
    {
        return effects.Sum(effect => effect.GetScore());
    }

    public override string ToString()
    {
        return string.Join("\n", effects.Select(effect => effect.EffectDescription()));
    }    
}

public class BattleMoveEffect : IBattleEffect
{
    public BattleCharacter character;
    public Vector3Int position;
    private TaskCompletionSource<bool> moveTcs;

    public BattleMoveEffect(BattleCharacter character, Vector3Int position)
    {
        this.character = character;
        this.position = position;
    }
    public async Task DoEffect()
    {
        moveTcs = new TaskCompletionSource<bool>();
        Vector3 targetPos = BattleBlockManager.Instance.tilemap.CellToWorld(position);
        character.View.transform.DOMove(targetPos, 1f).OnComplete(() => moveTcs.TrySetResult(true));
        character.Position = position;
        await moveTcs.Task;
    }

    public int GetScore()
    {
        return 0;
    }

    public string EffectDescription()
    {
        return "移动";
    }
}