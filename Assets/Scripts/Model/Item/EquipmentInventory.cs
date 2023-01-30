using System;
using Model;
using System.Collections.Generic;

public class EquipmentInventory : ItemInventory<Item, EquipmentItemStack>
{
    private Dictionary<EquipmentType, ItemStack> equipDict = new Dictionary<EquipmentType, ItemStack>();

    public EquipmentInventory() : base(0)
    {
        Capacity = Enum.GetValues(typeof(EquipmentType)).Length;
    }

    // 装备物品， 并返回原装备的物品
    public bool EquipItem(Item item, out Item original)
    {
        ItemStack stack = FindEquipStack(item);
        if (stack == null)
        {
            original = null;
            return false;
        }
        if (!stack.Empty)
        {
            original = stack.item;
            stack.DiscardItem(1);
        }
        else
        {
            original = null;
        }
        if (stack.StoreItem(item))
        {
            return true;
        }
        else
        {
            original = null;
            stack.StoreItem(original);
            return false;
        }
    }

    public override bool StoreItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        ItemStack stack = FindEquipStack(item);
        if (stack == null)
        {
            return false;
        }
        return stack.StoreItem(item);
    }

    public override bool StoreItem(Item item, int num)
    {
        if (num > 1)
        {
            return false; // 装备无法堆叠
        }
        return base.StoreItem(item, num);
    }

    public ItemStack FindEquipStack(Item item)
    {
        if (item == null || item.ItemType != ItemType.Equipment)
        {
            return null;
        }
        EquipmentType equipmentType = (EquipmentType)item.SubItemType;
        if (equipDict.ContainsKey(equipmentType))
        {
            return equipDict[equipmentType];
        }
        else
        {
            EquipmentItemStack itemstack = new EquipmentItemStack();
            itemstack.equipmentType = equipmentType;
            itemstack.OnStackChange.AddListener(OnItemStackChange);
            Stacks.Add(itemstack);
            equipDict.Add(equipmentType, itemstack);
            return itemstack;
        }
    }

    protected override ItemStack InitalizeNewStack()
    {
        return null;
    }
}