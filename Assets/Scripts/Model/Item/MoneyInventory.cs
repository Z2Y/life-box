
using System.Linq;
using System.Collections.Generic;
using Model;
using ModelContainer;
public class MoneyStack : InfiniteItemStack {
    public override bool StoreItem(Item other, int num)
    {
        if (other.ItemType == ItemType.Money)
        {
            return base.StoreItem(other, num);
        }
        else
        {
            return false;
        }
    }
}

public class MoneyInventory : ItemInventory<Item, MoneyStack>
{
    public Item DefaultMoneyItem { get; set;}
    public MoneyInventory() : base(5) {
        List<Item> moneyItems = ItemCollection.Instance.GetItemsByType(ItemType.Money);
        if (moneyItems != null) {
            DefaultMoneyItem = moneyItems.FirstOrDefault();
        }
    }

    public void BindToWealth(PropertyValue property) {
        if (DefaultMoneyItem == null) { return; }

        StoreItem(DefaultMoneyItem, (int)(property.value / DefaultMoneyItem.Wealth));
        property.Type.SetFrozen(true);
        onInventoryChange.AddListener(() => {
            property.value = (int)Stacks.Sum((stack) => stack.item.Wealth * stack.Count);
            property.owner.onPropertyChange?.Invoke();
        });
    }
}