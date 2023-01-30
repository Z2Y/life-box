using System;
using System.Collections.Generic;
using Model;

public abstract class ItemUsageEffect
{
    public abstract void TackEffect(Character player);
    public abstract string UsageDescription();
}

public class ItemUsageCache : Singleton<ItemUsageCache>
{
    public Dictionary<long, ItemUsage> usages = new Dictionary<long, ItemUsage>();

    public ItemUsage EmptyUsage = new ItemUsage();

    public ItemUsage GetUsage(Item item)
    {
        ItemUsage result;
        if (usages.TryGetValue(item.ID, out result))
        {
            return result;
        }
        else
        {
            result = item.EffectDescription.ExecuteExpression() as ItemUsage;
            usages.Add(item.ID, result);
            return result;
        }
    }
}