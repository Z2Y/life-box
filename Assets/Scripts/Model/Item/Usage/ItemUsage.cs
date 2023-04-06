using System.Collections.Generic;
using Cathei.LinqGen;
using Model;

public interface IItemUsable {
    string UsageDescription();
    void Use();
}

public class ItemUsage {
    public int UseLimt; // 使用次数
    public bool DestroyOnUseup;
    public List<ItemUsageEffect> effects = new List<ItemUsageEffect>();

    public string cachedDescription;

    public string Description
    {
        get {
            if (cachedDescription == null) {
                cachedDescription = string.Join("\n", effects.Gen().Select((effect) => effect.UsageDescription()).AsEnumerable() );
            }
            return cachedDescription;
        }
    }

    public void Use(Character player) {
        foreach(var effect in effects) {
            effect.TackEffect(player);
        }
    }

    public bool Usable()
    {
        return effects.Count > 0;
    }
}

public class UsableItemStack : ItemStack, IItemUsable
{
    public int used; // 已经使用的次数
    public int usedItems; // 已经使用的物品数量
    public ItemUsage usage;
    private int capacity;

    public string UsageDescription()
    {
        if (item.Effect.Length <= 0) return item.Description;
        return $"{item.Description} \n使用效果\n{usage.Description}";
    }

    public bool Usable() {
        return !Empty && usage.Usable() && usedItems < count;
    }

    public void Use()
    {
        if (!Usable()) return;
        if (used < usage.UseLimt) {
            usage.Use(null);
            used += 1;
        }

        if (used >= usage.UseLimt) {
            if (usage.DestroyOnUseup) {
                DiscardItem(1);
            } else {
                usedItems += 1;
            }
            used = 0;
        }
    }

    public void SetCapacity(int value)
    {
        capacity = value;
    }

    public override int Capacity {
        get {
            if (capacity == 0) return base.Capacity;
            return capacity;
        }
    }

    public override bool StoreItem(Item other, int num)
    {
        if (other == null || num <= 0) return false;
        if (usage != null && item.ID == other.ID)
        {
            if(base.StoreItem(other, num)) {
                return true;
            }
            return false;
        }
        else
        {
            ItemUsage otherUsage = ItemUsageCache.Instance.GetUsage(other);
            if (otherUsage != null && base.StoreItem(other, num)) {
                usage = otherUsage;
                return true;
            }
            return false;
        }
    }
}