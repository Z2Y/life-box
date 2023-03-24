using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CommandResolverHandler("BattlePropertyCost")]
public class BattlePropertyCostResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        BattleSkillAction skillAction = env["Skill"] as BattleSkillAction;
        if (skillAction == null) return null;

        if (Enum.TryParse<SubPropertyType>(args[0] as string, true, out var propertyType))
        {
            int costValue = Convert.ToInt32(args[1]);
            BattlePropertyCost cost = new BattlePropertyCost(skillAction.self, propertyType, costValue);
            return cost;
        }
        await this.Done();
        return null;
    }
}

[CommandResolverHandler("BattleCost")]
public class BattleCostResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var result = new BattleCostResult
        {
            costs = env.Values.OfType<IBattleCost>().ToList()
        };
        await this.Done();
        return result;
    }
}

public class BattleCostResult
{
    public List<IBattleCost> costs = new ();

    public bool CouldCost()
    {
        return costs.Count <= 0 || costs.All(cost => cost.CouldCost());
    }

    public void Cost()
    {
        foreach(var cost in costs) {
            cost.Cost();
        }
    }

    public override string ToString()
    {
        return string.Join("\n", costs.Select(cost => cost.CostDescription()));
    }
}

public interface IBattleCost
{
    bool CouldCost();
    void Cost();
    string CostDescription();
}

public class BattlePropertyCost : IBattleCost
{
    private readonly BattleCharacter character;
    private readonly SubPropertyType propertyType;
    private readonly int value;

    public BattlePropertyCost(BattleCharacter character, SubPropertyType propertyType, int value)
    {
        this.character = character;
        this.propertyType = propertyType;
        this.value = value;
    }

    public bool CouldCost()
    {
        return character != null && character.Property.GetProperty(propertyType).value > value;
    }

    public void Cost()
    {
        var propertyValue = character?.Property.GetProperty(propertyType);
        if (propertyValue == null) return;
        propertyValue.value -= value;
    }

    public string CostDescription()
    {
        return $"消耗 {propertyType.GetPropertyName()} {value}";
    }
}