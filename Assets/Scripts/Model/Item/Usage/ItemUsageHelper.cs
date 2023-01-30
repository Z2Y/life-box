using System;
using Model;

public static class ItemUsageHelper {
    public static UsableItemStack UsableStack(this ItemStack stack)
    {
        if (stack is UsableItemStack) return stack as UsableItemStack;
        if (stack.Empty) return null;
        ItemUsage usage = ItemUsageCache.Instance.GetUsage(stack.item);
        if (usage != null) {
            UsableItemStack usableItemStack = new UsableItemStack();
            usableItemStack.SetCapacity(stack.Capacity);
            usableItemStack.SetItem(stack.item);
            usableItemStack.StoreItem(stack.item, stack.Count);
            return usableItemStack;
        }
        return null;
    }
}