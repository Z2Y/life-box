using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;

namespace Model
{

    [MessagePackObject(true)]
    public class ShopConfig
    {
        public long ID;
        public string Name;
        public long CharacterID;
        public long Currency;
        public int MaxItemCount;
        public string ShopCondition;
        public long[] Item;
        public int[] RefreshCount;
        public string[] Condition;
        public int[] Recycle;
        public int[] Price;

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
    [ModelContainerOf(typeof(Model.ShopConfig), "configs")]
    public class ShopConfigCollection
    {
        private Dictionary<long, Model.ShopConfig> lookup = new Dictionary<long, Model.ShopConfig>();
        private Dictionary<long, List<Model.ShopConfig>> lookupByCharacter = new Dictionary<long, List<Model.ShopConfig>>();
        private List<Model.ShopConfig> configs = new List<Model.ShopConfig>();
        private static ShopConfigCollection _instance;
        private ShopConfigCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Model.ShopConfig config in configs) {
                if (isValidConfig(config)) {
                    lookup.Add(config.ID, config);
                } else {
                    UnityEngine.Debug.LogWarning($"Invalid Shop Config Found. Check {config.ID}");
                }
            }
            lookupByCharacter = lookup.Values.GroupBy((config) => config.CharacterID).ToDictionary((group) => group.Key, (group) => group.ToList());
        }

        private bool isValidConfig(Model.ShopConfig config) {
            int itemCount = config.Item.Length;
            return config.Price.Length == itemCount && config.Condition.Length >= itemCount && config.Recycle.Length == itemCount;
        }

        public Model.ShopConfig GetShopConfig(long id)
        {
            Model.ShopConfig value;
            if (lookup.TryGetValue(id, out value))
            {
                return value;
            }
            return null;
        }

        public List<Model.ShopConfig> GetShopConfigsByCharacter(long characterID) {
            List<Model.ShopConfig> configs;
            if (lookupByCharacter.TryGetValue(characterID, out configs))
            {
                return configs;
            }
            return null;            
        }

        public static ShopConfigCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ShopConfigCollection();
                }
                return _instance;
            }
        }

    }
}