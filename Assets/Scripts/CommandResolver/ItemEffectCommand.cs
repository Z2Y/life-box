using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using Utils;

[CommandResolverHandler("ItemUsage")]
public class ItemUsageResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var result = new ItemUsage
        {
            effects = env.Values.Gen().OfType<ItemUsageEffect>().GetEnumerator().ToList(),
            UseLimt = Convert.ToInt32(args[0]),
            DestroyOnUseup = Convert.ToInt32(args[1]) > 0
        };
        await this.Done();
        return result;
    }    
}

[CommandResolverHandler("ChangePropertyEffect")]
public class ChangePropertyEffectResolver : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        ItemChangePropertyEffect result = new ItemChangePropertyEffect();
        for (int i = 0; i < args.Count; i += 2)
        {
            int propertyChangeValue = Convert.ToInt32(args[i + 1]);
            if (Enum.TryParse<SubPropertyType>(args[i] as string, true, out var propertyType)) {
                if (result.changes.ContainsKey(propertyType)) {
                    result.changes[propertyType] += propertyChangeValue;
                } else {
                    result.changes.Add(propertyType, propertyChangeValue);
                }
            }
        }
        await this.Done();
        return result;
    }
}