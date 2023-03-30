using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Model;
using ModelContainer;

public abstract class ItemCommandResolver : CommandResolver {
    protected ItemInventory Knapsack => LifeEngine.Instance.lifeData?.knapsackInventory;

    protected MoneyInventory Wallet => LifeEngine.Instance.lifeData?.moneyInventory;
}

public class ItemCommandResult {
    public readonly List<ItemStack> Items = new ();

    public override string ToString()
    {
        return string.Join(" ", Items.Select(stack => $"【{stack.item.Name} x {stack.Count}】"));
    }
}

[CommandResolverHandler("ReceiveItem")]
public class ReceiveItemCommand : ItemCommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        ItemCommandResult received = new ItemCommandResult();
        for (int i = 0; i < args.Count; i += 2)
        {
            ItemStack stack = ReceiveItem(Convert.ToInt64(args[i]), Convert.ToInt32(args[i + 1]));
            if (stack != null && !stack.Empty) {
                received.Items.Add(stack);
            }
        }
        await this.Done();
        return received;
    }

    private ItemStack ReceiveItem(long itemId, int count)
    {
        Item item = ItemCollection.GetItem(itemId);
        if (item == null) return null;
        ItemStack received = new InfiniteItemStack();
        received.StoreItem(item, count);
        bool success = false;
        if (item.ItemType == (int)ItemType.Money) {
            success = Wallet.StoreItem(item, count);
        } else {
            success = Knapsack.StoreItem(item, count);
        }
        return success ? received : null;
    }
}

[CommandResolverHandler("LostItem")]
public class LostItemCommand : ItemCommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var lost = new ItemCommandResult();
        for (var i = 0; i < args.Count; i += 2)
        {
            var stack = LostItem(Convert.ToInt64(args[i]), Convert.ToInt32(args[i + 1]));
            if (stack != null && !stack.Empty) {
                lost.Items.Add(stack);
            }
        }
        await this.Done();
        return lost;
    }

    private ItemStack LostItem(long itemId, int count)
    {
        var item = ItemCollection.GetItem(itemId);
        if (item == null) return null;
        ItemStack lost = new InfiniteItemStack();
        lost.StoreItem(item, count);
        var success = item.ItemType == (int)ItemType.Money ? Wallet.DiscardItem(item, count) : Knapsack.DiscardItem(item, count);
        return success ? lost : null;
    }
}

[CommandResolverHandler("CountItem")]
public class CountItemCommand : ItemCommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        await this.Done();
        return CountItem(Convert.ToInt64(args[0]));
    }

    private int CountItem(long itemId) {
        var item = ItemCollection.GetItem(itemId);
        if (item == null) return 0;
        return item.ItemType == (int)ItemType.Money ? Wallet.CountItem(item) : Knapsack.CountItem(item);        
    }
}

[CommandResolverHandler("OpenKnapsack")]
public class OpenKnapsackCommand : CommandResolver
{
    public override async UniTask<object> Resolve(string arg, List<object> args, Dictionary<string, object> env)
    {
        var knapsack = LifeEngine.Instance.lifeData?.knapsackInventory;
        await KnapsackPanel.Show(knapsack);
        return null;
    }
}

