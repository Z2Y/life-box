using UnityEngine;

namespace Logic.Loot
{
    public interface ILootItem
    {
        public ItemStack GetLootItemStack();

        public void OnLoot(GameObject by);
    }
}