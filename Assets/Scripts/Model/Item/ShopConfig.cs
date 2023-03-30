using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using Realms;

namespace Model
{

    [MessagePackObject(true)]
    [Serializable]
    public partial class ShopConfig : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string Name { get; set; }
        public long CharacterID { get; set; }
        public long Currency { get; set; }
        public int MaxItemCount { get; set; }
        public string ShopCondition { get; set; }
        public IList<long> Item { get; }
        public IList<int> RefreshCount { get; }
        public IList<string> Condition { get; }
        public IList<int> Recycle { get; }
        public IList<int> Price { get; }

        public bool isOpen {
            get {
                if (ShopCondition.Length <= 0) return true;
                bool? open = ShopCondition.ExecuteExpression() as bool?;
                return open != null && (bool)open;
            }
        }
    }
}

namespace ModelContainer
{
    public static class ShopConfigCollection
    {
        public static bool IsValid(this ShopConfig config) {
            int itemCount = config.Item.Count;
            return config.Price.Count == itemCount && config.Condition.Count >= itemCount && config.Recycle.Count == itemCount;
        }

        public static ShopConfig GetShopConfig(long id)
        {
            return RealmDBController.Db.Find<ShopConfig>(id);
        }

        public static IQueryable<ShopConfig> GetShopConfigsByCharacter(long characterID)
        {
            return RealmDBController.Db.All<ShopConfig>().Where((config) => config.CharacterID == characterID);
        }
    }
}