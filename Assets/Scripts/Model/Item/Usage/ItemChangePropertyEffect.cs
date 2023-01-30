using System;
using System.Linq;
using System.Collections.Generic;
using Model;

public class ItemChangePropertyEffect : ItemUsageEffect
{
    public Dictionary<SubPropertyType, int> changes = new Dictionary<SubPropertyType, int>();

    public override void TackEffect(Character player)
    {
        ICommandResolver resolver =  ExpressionCommandResolver.GetResolver("ChangeProperty");

        List<object> args = new List<object>();

        foreach(var change in changes) {
            args.Add(change.Key.ToString());
            args.Add(change.Value);
        }

        resolver.Resolve("", args, null);
    }

    public override string UsageDescription()
    {
        return string.Join("\n", changes.Keys.Select((propertyType) => $"{propertyType.GetPropertyName()} {(changes[propertyType] > 0 ? "+" : "-")}{changes[propertyType]}"));
    }
}