using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

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
    [ModelContainerOf(typeof(Model.Item), "items")]
    public class ItemCollection
    {
        private Dictionary<long, Model.Item> lookup = new Dictionary<long, Model.Item>();
        private Dictionary<Model.ItemType, List<Model.Item>> lookupByType = new Dictionary<Model.ItemType, List<Model.Item>>();
        private List<Model.Item> items = new List<Model.Item>();
        private static ItemCollection _instance;
        private ItemCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Model.Item evt in items) {
                lookup.Add(evt.ID, evt);
            }
            lookupByType = items.GroupBy((item) => item.ItemType).ToDictionary((group) => group.Key, (group) => group.ToList());
        }

        public Model.Item GetItem(long id)
        {
            Model.Item value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public List<Model.Item> GetItemsByType(Model.ItemType itemType) {
            List<Model.Item> items;
            if (lookupByType.TryGetValue(itemType, out items))
            {
                return items;
            }
            return null;            
        }

        public static ItemCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemCollection();
                }
                return _instance;
            }
        }

        public static IEnumerable<int> GetValidItemIndex(IEnumerable<long> events) {
            return events.Select((long id, int idx) =>
            {
                Model.Item e = Instance.GetItem(id);
                if (e == null) return -1;
                return idx;
            }).Where((int v) => v >= 0);            
        }

        public static IEnumerable<Model.Item> GetValidItems(long[] ids) {
            return GetValidItemIndex(ids).Select((idx) => Instance.GetItem(ids[idx]));
        }

        public static int RandomItemIndex(long[] ids, float[] weights) {
            IEnumerable<int> validItems = GetValidItemIndex(ids);
            if (validItems.Count() == 0) { return -1; }
            float targetW = UnityEngine.Random.Range(0, validItems.Select((int idx) => weights[idx]).Sum());
            float currentW = 0;
            return validItems.FirstOrDefault((int idx) => {
                currentW += weights[idx];
                return currentW > targetW;
            });
        }

        public static Model.Item RandomItem(long[] ids, float[] weights) {
            int index = RandomItemIndex(ids, weights);
            if (index < 0) return null;
            return Instance.GetItem(ids[index]);
        }
    }
}