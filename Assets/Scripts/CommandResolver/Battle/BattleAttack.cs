using System;
using UnityEngine;
using DG.Tweening;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

[CommandResolverHandler("BattleNormalAttack")]
public class BattleNormalAttack : CommandResolver
{
    public TaskCompletionSource<bool> moveTcs;
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        BattleSkillAction skillAction = env["Skill"] as BattleSkillAction;
        if (skillAction == null || skillAction.target == null) return null;
        BattleNormalAttackEffect result = new BattleNormalAttackEffect(skillAction.skill, skillAction.self, skillAction.target);
        await this.Done();
        return result;
    }
}

public enum DamageType
{
    Normal, // 普通伤害
    Sword, // 剑系伤害
}

public class BattleAttackDamage
{
    public DamageType type;
    public int value;

    public BattleAttackDamage(int value, DamageType type)
    {
        this.value = value;
        this.type = type;
    }
}

public class BattleNormalAttackEffect : IBattleEffect
{
    public Skill skill;
    public BattleCharacter self;
    public BattleCharacter target;

    public BattleNormalAttackEffect(Skill skill, BattleCharacter self, BattleCharacter target)
    {
        this.skill = skill;
        this.self = self;
        this.target = target;
    }
    public async Task DoEffect()
    {
        int damage = skill.Attack;
        target.Hp.value -= damage;
        target.UpdateHpSlider();
        if (damage > 0) {
            Vector3 jumpDirection = target.View.transform.position - self.View.transform.position;
            target.View.ShowAttackInfo($"<color=#FD5C36>-{damage}</color>", jumpDirection.normalized);
        }
        await YieldCoroutine.Instance.WaitForInstruction(new WaitForEndOfFrame());
    }

    public int GetScore()
    {
        return skill.Attack;
    }

    public string EffectDescription()
    {
        return $"对目标照成 {skill.Attack}点 伤害。";
    }
}

public class BattleAttackResult
{
    public Dictionary<BattleCharacter, BattleAttackDamage> damages = new Dictionary<BattleCharacter, BattleAttackDamage>();
}