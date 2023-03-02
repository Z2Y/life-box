using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using Model;
public class ItemStack
{
    public Item item { get; protected set; }

    protected int count;

    protected readonly ItemStackChangeEvent onStackChange = new ItemStackChangeEvent();

    public ItemStackChangeEvent OnStackChange => onStackChange;

    public int Count
    {
        get
        {
            return count;
        }
        protected set
        {
            count = value;
            OnStackChange?.Invoke();
        }
    }

    public virtual int Capacity
    {
        get
        {
            if (item == null)
            {
                return 1;
            }
            else
            {
                return item.StackCount;
            }
        }
    }

    public bool SetItem(Item other)
    {
        if (item == null)
        {
            item = other;
            Count = 0;
            return true;
        }
        return false;
    }

    public bool Empty => Count <= 0;

    public bool Full => Count >= Capacity;

    public bool DiscardItem(int num)
    {
        if (item == null || Count < num)
        {
            return false;
        }
        Count -= num;
        if (Count == 0)
        {
            item = null;
        }
        return true;
    }

    public ItemStack SplitItem(int num)
    {
        if (item == null || Count < num)
        {
            return null;
        }
        ItemStack other = new ItemStack();
        other.item = item;
        other.Count = num;
        Count -= num;
        if (Count == 0)
        {
            item = null;
        }
        return other;
    }

    public bool StoreItem(Item other)
    {
        return StoreItem(other, 1);
    }

    public virtual bool StoreItem(Item other, int num)
    {
        if (num > other.StackCount || num == 0)
        {
            return false;
        }
        if (item == null)
        {
            item = other;
            Count = num;
            return true;
        }
        if (item.ID != other.ID || Count + num > Capacity)
        {
            return false;
        }
        Count += num;
        return true;
    }
}

public class UniqueItemStack : ItemStack
{
    public override int Capacity => 1;
}

public class InfiniteItemStack : ItemStack
{
    public override int Capacity => int.MaxValue;

    public override bool StoreItem(Item other, int num)
    {
        if (num == 0)
        {
            return false;
        }
        if (item == null)
        {
            item = other;
            Count = num;
            return true;
        }
        if (item.ID != other.ID)
        {
            return false;
        }
        Count += num;
        return true;
    }
}

public class ItemStackChangeEvent : UnityEvent { }