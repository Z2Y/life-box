using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CommandResolverHandler("BattleMove")]
public class BattleMove : CommandResolver
{
    public UniTaskCompletionSource<bool> moveTcs;
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
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
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var result = new BattleEffectResult
        {
            effects = env.Values.OfType<IBattleEffect>().ToList()
        };
        await this.Done();
        return result;
    }
}

public interface IBattleEffect
{
    UniTask DoEffect();
    string EffectDescription();
    int GetScore();
}

public class BattleEffectResult
{
    public List<IBattleEffect> effects = new ();

    public async UniTask DoEffect()
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
    private readonly BattleCharacter character;
    public Vector3Int position;
    private UniTaskCompletionSource<bool> moveTcs;

    public BattleMoveEffect(BattleCharacter character, Vector3Int position)
    {
        this.character = character;
        this.position = position;
    }
    public async UniTask DoEffect()
    {
        moveTcs = new UniTaskCompletionSource<bool>();
        var targetPos = BattleBlockManager.Instance.tilemap.CellToWorld(position);
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