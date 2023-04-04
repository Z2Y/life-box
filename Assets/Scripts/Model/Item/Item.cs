using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using MessagePack;
using Model;
using Realms;

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
    public partial class Item : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        [Indexed]
        public int ItemType { get; set; }
        public int SubItemType { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public float Wealth { get; set; }
        public long Unique { get; set; }
        public int StackCount { get; set; }
        public string Effect { get; set; }
        public string EffectDescription { get; set; }
        public string WorldSprite { get; set; }
        public string IconSprite { get; set; }
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Item), "items")]
    public static class ItemCollection
    {

        public static Item GetItem(long id)
        {
            return RealmDBController.Db.Find<Item>(id);
        }

        public static IEnumerable<Item> GetItemsByType(ItemType itemType)
        {
            return RealmDBController.Db.All<Item>().Where((item) => item.ItemType == (int)itemType);
        }

        public static Item FirstItemOfType(ItemType itemType)
        {
            return RealmDBController.Db.All<Item>().FirstOrDefault(item => item.ItemType == (int)itemType);
        }

        /*
        private static IEnumerable<long> GetValidItemIndex(IEnumerable<long> ids)
        {
            return RealmDBController.Db.All<Item>().Filter("ID IN {%@}", string.Join(",", ids)).ToList()
                .Select((item) => item.ID);
        }

        private static long RandomItemIndex(long[] ids, float[] weights) {
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
            var index = RandomItemIndex(ids, weights);
            return index < 0 ? null : GetItem(ids[index]);
        }*/
    }
}