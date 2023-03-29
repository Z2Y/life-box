using UnityEngine;

namespace Logic.Loot
{
    public interface ILootItem
    {
        public bool IsAutoLoot();
        public ItemStack GetLootItemStack();

        public void OnLoot(GameObject by);
    }
}