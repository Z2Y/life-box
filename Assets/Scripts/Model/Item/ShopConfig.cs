using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{

    [MessagePackObject(true)]
    [Serializable]
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
    [ModelContainerOf(typeof(ShopConfig), "configs")]
    public class ShopConfigCollection
    {
        private readonly Dictionary<long, ShopConfig> lookup = new ();
        private Dictionary<long, List<ShopConfig>> lookupByCharacter = new ();
        private List<ShopConfig> configs = new ();
        private static ShopConfigCollection _instance;
        private ShopConfigCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(ShopConfig config in configs) {
                if (isValidConfig(config)) {
                    lookup.TryAdd(config.ID, config);
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
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public List<ShopConfig> GetShopConfigsByCharacter(long characterID)
        {
            return lookupByCharacter.TryGetValue(characterID, out var shopConfigs) ? shopConfigs : null;
        }

        public static ShopConfigCollection Instance => _instance ??= new ShopConfigCollection();
    }
}