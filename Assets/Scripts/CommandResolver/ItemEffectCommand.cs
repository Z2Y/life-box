using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model;
using ModelContainer;

[CommandResolverHandler("ItemUsage")]
public class ItemUsageResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        ItemUsage result = new ItemUsage();
        result.effects = env.Values.Where((value) => value is ItemUsageEffect).Select((value) => value as ItemUsageEffect).ToList();
        result.UseLimt = Convert.ToInt32(args[0]);
        result.DestroyOnUseup = Convert.ToInt32(args[1]) > 0;
        await this.Done();
        return result;
    }    
}

[CommandResolverHandler("ChangePropertyEffect")]
public class ChangePropertyEffectResolver : CommandResolver
{
    public override async Task<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
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