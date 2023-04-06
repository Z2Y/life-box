using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using Model;

public class ItemChangePropertyEffect : ItemUsageEffect
{
    public readonly Dictionary<SubPropertyType, int> changes = new ();

    public override void TackEffect(Character player)
    {
        List<object> args = new List<object>();

        foreach(var change in changes) {
            args.Add(change.Key.ToString());
            args.Add(change.Value);
        }

        ExpressionCommandResolver.Resolve("ChangeProperty", "", args, null).Forget();
    }

    public override string UsageDescription()
    {
        return string.Join("\n", 
            changes.Keys.Gen().Select((propertyType) => $"{propertyType.GetPropertyName()} {(changes[propertyType] > 0 ? "+" : "-")}{changes[propertyType]}").AsEnumerable());
    }
}