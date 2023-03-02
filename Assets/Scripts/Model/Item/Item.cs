using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{

    public enum ItemType {
        Food = 0,
        Money = 1,
        Equipment = 2,
        Book = 3,
        material = 4,
        Tool = 5,
        Special = 6
    }

    [MessagePackObject(true)]
    [Serializable]
    public class Item
    {
        public long ID;
        public ItemType ItemType;
        public int SubItemType;
        public string Description;
        public string Name;
        public float Wealth;
        public long Unique;
        public int StackCount;
        public string Effect;
        public string EffectDescription;
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Item), "items")]
    public class ItemCollection
    {
        private readonly Dictionary<long, Item> lookup = new ();
        private Dictionary<ItemType, List<Item>> lookupByType = new ();
        private List<Item> items = new ();
        private static ItemCollection _instance;
        private ItemCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(var evt in items) {
                lookup.Add(evt.ID, evt);
            }
            lookupByType = items.GroupBy((item) => item.ItemType).ToDictionary((group) => group.Key, (group) => group.ToList());
        }

        public Item GetItem(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<Item> GetItemsByType(ItemType itemType)
        {
            return lookupByType.TryGetValue(itemType, out var byType) ? byType : null;
        }

        public static ItemCollection Instance => _instance ??= new ItemCollection();

        public static IEnumerable<int> GetValidItemIndex(IEnumerable<long> events) {
            return events.Select((id, idx) =>
            {
                var e = Instance.GetItem(id);
                if (e == null) return -1;
                return idx;
            }).Where((v) => v >= 0);            
        }

        public static IEnumerable<Item> GetValidItems(long[] ids) {
            return GetValidItemIndex(ids).Select((idx) => Instance.GetItem(ids[idx]));
        }

        public static int RandomItemIndex(long[] ids, float[] weights) {
            var validItems = GetValidItemIndex(ids).ToList();
            if (!validItems.Any()) { return -1; }
            float targetW = UnityEngine.Random.Range(0, validItems.Select((idx) => weights[idx]).Sum());
            float currentW = 0;
            return validItems.FirstOrDefault((idx) => {
                currentW += weights[idx];
                return currentW > targetW;
            });
        }

        public static Item RandomItem(long[] ids, float[] weights) {
            int index = RandomItemIndex(ids, weights);
            if (index < 0) return null;
            return Instance.GetItem(ids[index]);
        }
    }
}