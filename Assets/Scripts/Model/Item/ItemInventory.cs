using System;
using System.Linq;
using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.Events;


public class ItemInventory
{
    public List<ItemStack> Stacks { get; protected set; }

    public int Capacity = default;

    protected InventoryChangeEvent onInventoryChange = new InventoryChangeEvent();

    public InventoryChangeEvent OnInventoryChange
    {
        get
        {
            return onInventoryChange;
        }
    }

    protected bool isStoring = false;

    public ItemInventory(int capacity)
    {
        this.Stacks = new List<ItemStack>();
        this.Capacity = capacity;
    }

    public virtual ItemStack GetStack(long itemId)
    {
        foreach (ItemStack stack in Stacks)
        {
            if (!stack.Empty && stack.item.ID == itemId)
            {
                return stack;
            }
        }
        return null;
    }

    public virtual bool StoreItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        ItemStack stack = null;
        if (item.StackCount == 1)
        {
            stack = FindEmptyStack();
        }
        else
        {
            stack = FindItemStack(item);
        }
        if (stack == null)
        {
            return false;
        }
        return stack.StoreItem(item);
    }

    public virtual bool ReplaceStack(int index, ItemStack stack)
    {
        if (index < 0 || index >= Stacks.Count || Stacks[index] == stack)
        {
            return false;
        }
        Stacks[index].OnStackChange.RemoveListener(OnItemStackChange);
        stack.OnStackChange.AddListener(OnItemStackChange);
        Stacks[index] = stack;
        OnInventoryChange?.Invoke();
        return true;
    }

    public virtual bool StoreItem(Item item, int num)
    {
        if (num == 1)
        {
            return StoreItem(item);
        }
        if (item == null || num <= 0)
        {
            return false;
        }
        isStoring = true;

        int capacity = 0;
        List<ItemStack> avaliableStacks = FindAvaliableItemStacks(item);
        foreach (ItemStack stack in avaliableStacks)
        {
            if (stack.Empty)
            {
                stack.SetItem(item);
                capacity += stack.Capacity;
            }
            else
            {
                capacity += stack.Capacity - stack.Count;
            }
            if (capacity >= num)
            {
                break;
            }
        }
        while (capacity < num && Stacks.Count < this.Capacity)
        {
            ItemStack stack = InitalizeNewStack();
            stack.SetItem(item);
            avaliableStacks.Add(stack);
            capacity += stack.Capacity;
        }
        if (capacity < num)
        {
            return false;
        }
        foreach (ItemStack stack in avaliableStacks)
        {
            int stacked = 0;
            if (stack.Empty)
            {
                stacked = Mathf.Min(num, stack.Capacity);
                stack.StoreItem(item, stacked);
                num -= stacked;
            }
            else
            {
                stacked = Mathf.Min(num, stack.Capacity - stack.Count);
                stack.StoreItem(item, stacked);
                num -= stacked;
            }
            if (num <= 0)
            {
                break;
            }
        }
        isStoring = false;
        OnInventoryChange?.Invoke();
        return true;
    }

    public virtual bool DiscardItem(Item item, int num) {
        if (item == null || num <= 0) { return false;}
        List<ItemStack> stacks = Stacks.Where(s => (!s.Empty && s.item.ID == item.ID)).ToList();
        int total = stacks.Sum(s => s.Count);
        if (total < num) { return false; }
        isStoring = true;
        foreach (ItemStack stack in stacks)
        {
            int stacked = Mathf.Min(num, stack.Count);
            stack.DiscardItem(stacked);
            num -= stacked;
            if (num <= 0)
            {
                break;
            }
        }
        isStoring = false;
        OnInventoryChange?.Invoke();
        return true;                    
    }

    public virtual int CountItem(Item item) {
        if (item == null) { return 0; }
        return Stacks.Where(s => (!s.Empty && s.item.ID == item.ID)).Sum(s => s.Count);
    }

    public bool Empty {
        get {
            return Stacks.Count == 0 || Stacks.All((stack) => stack.Empty);
        }
    }

    protected List<ItemStack> FindAvaliableItemStacks(Item item)
    {
        return Stacks.Where(s => (s.Empty || (!s.Full && s.item.ID == item.ID))).ToList();
    }

    protected ItemStack FindItemStack(Item item)
    {
        ItemStack stack = Stacks.Where(s => (s.Empty || (!s.Full && s.item.ID == item.ID))).FirstOrDefault();
        if (stack == null)
        {
            stack = FindEmptyStack();
        }
        return stack;
    }

    protected ItemStack FindEmptyStack()
    {
        ItemStack stack = Stacks.Where(s => s.Empty).FirstOrDefault();
        if (stack == null)
        {
            stack = InitalizeNewStack();
        }
        return stack;
    }

    protected virtual ItemStack InitalizeNewStack()
    {
        if (Stacks.Count >= Capacity)
        {
            return null;
        }
        ItemStack stack = new ItemStack();
        stack.OnStackChange.AddListener(OnItemStackChange);
        Stacks.Add(stack);
        return stack;
    }

    protected void OnItemStackChange()
    {
        if (isStoring)
        {
            return;
        }
        OnInventoryChange?.Invoke();
    }
}

[Serializable]
public class ItemInventory<T> : ItemInventory where T : Item
{
    public ItemInventory(int capacity) : base(capacity) {
    }

    public override bool StoreItem(Item item)
    {
        if (item is T)
        {
            return base.StoreItem(item);
        }
        return false;
    }

    public override bool StoreItem(Item item, int num)
    {
        if (item is T)
        {
            return base.StoreItem(item, num);
        }
        return false;
    }
}

public class ItemInventory<T1, T2> : ItemInventory<T1> where T1 : Item where T2 : ItemStack, new()
{
    public ItemInventory(int capacity) : base(capacity) {
    }

    protected override ItemStack InitalizeNewStack()
    {
        if (Stacks.Count >= Capacity)
        {
            return null;
        }
        ItemStack stack = new T2();
        stack.OnStackChange.AddListener(OnItemStackChange);
        Stacks.Add(stack);
        return stack;
    }
}

public class InventoryChangeEvent : UnityEvent { }