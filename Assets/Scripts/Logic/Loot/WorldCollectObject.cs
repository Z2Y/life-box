using System;
using Model;
using UnityEngine;

namespace Logic.Loot
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldCollectObject : MonoBehaviour, ILootItem
    {
        [SerializeField] private bool isAutoLoot;
        
        [SerializeField] private Item item;

        [SerializeField] private int count;

        private ItemStack itemStack;

        private void Awake()
        {
            itemStack = new ItemStack();
            itemStack.StoreItem(item, count);
        }

        public bool IsAutoLoot()
        {
            return isAutoLoot;
        }

        public ItemStack GetLootItemStack()
        {
            return itemStack;
        }

        public void OnLoot(GameObject by)
        {
            Destroy(this);
        }
    }
}