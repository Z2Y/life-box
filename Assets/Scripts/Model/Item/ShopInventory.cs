using System;
using System.Collections.Generic;
using Model;
using ModelContainer;
using System.Linq;
using Utils.Shuffle;
using UnityEngine;

public class ShopItemStack : InfiniteItemStack
{
    public int Recycle;
    public int Price;
}

public class ShopInventory : ItemInventory<Item, ShopItemStack>
{

    public ShopConfig Config { get; private set; }

    public float SellRate = 1f;

    public float RecycleRate = 1f;

    private TimeSpan lastRefreshTime = new TimeSpan(-1, 0);

    private List<int> validItemIndex = new List<int>();

    public ShopInventory(ShopConfig config) : base(0)
    {
        Config = config;
        Capacity = config.Item.Length;
    }

    public bool StoreItem(Item other, int num, int price, int resycle)
    {
        ShopItemStack stack = GetStack(other.ID) as ShopItemStack;
        if (stack != null && stack.Recycle == resycle && stack.Price == price) {
            return stack.StoreItem(other, num);
        } else {
            stack = InitalizeNewStack() as ShopItemStack;
            stack.Price = price;
            stack.Recycle = resycle;
            return stack.StoreItem(other, num);
        }
    }

    public int GetResycle(long itemID)
    {
        int itemIndex = validItemIndex.FindIndex((idx) => Config.Item[idx] == itemID);
        if (itemIndex < 0) return 0;
        return Config.Recycle[validItemIndex[itemIndex]];
    }

    public void RefreshShopItem(TimeSpan time, bool force = false)
    {
        if (!force && time == lastRefreshTime) return;
        validItemIndex = Config.Item.Select((_, idx) => isConditionValid(Config.Condition[idx]) ? idx : -1).Where((idx) => idx >= 0).ToList();
        validItemIndex = validItemIndex.Shuffle().Take(Config.MaxItemCount).ToList();
        isStoring = true;
        for (int i = 0; i < validItemIndex.Count; i++)
        {
            Item item = ItemCollection.Instance.GetItem(Config.Item[validItemIndex[i]]);
            int RefreshCount = Config.RefreshCount[validItemIndex[i]];
            int Price = Config.Price[validItemIndex[i]];
            int Resycle = Config.Recycle[validItemIndex[i]];
            ShopItemStack stack;
            if (i < Stacks.Count) {
                stack = Stacks[i] as ShopItemStack;
                stack.DiscardItem(stack.Count);
            } else {
                stack = InitalizeNewStack() as ShopItemStack;
            }
            stack.StoreItem(item, RefreshCount);
            stack.Price = (int)(Price * SellRate);
            stack.Recycle = (int)(Resycle * RecycleRate);
        }
        isStoring = false;
        lastRefreshTime = time;
        onInventoryChange?.Invoke();
    }

    private bool isConditionValid(string sellCondition)
    {
        if (sellCondition == null || sellCondition.Length <= 0) return true;
        bool? result = sellCondition.ExecuteExpression() as bool?;
        if (result == null) return false;
        return (bool)result;
    }

    public Item Currency => ItemCollection.Instance.GetItem(Config.Currency);
}

public class ShopInventoryCollection : Singleton<ShopInventoryCollection> {
    private Dictionary<long, ShopInventory> shops = new Dictionary<long, ShopInventory>();

    public ShopInventory GetInventory(ShopConfig config) {
        if (shops.TryGetValue(config.ID, out var inventory)) {
            return inventory;
        }

        inventory = new ShopInventory(config);
        shops.Add(config.ID, inventory);
        return inventory;
    }

    public bool RemoveInventory(long configID) {
        return shops.Remove(configID);
    }
}