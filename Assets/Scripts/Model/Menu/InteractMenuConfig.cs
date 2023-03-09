using System;
using System.Collections.Generic;
using MessagePack;
using InteractMenuConfig = Model.InteractMenuConfig;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public class InteractMenuConfig
    {
        public long ID;
        public string Name;
        public string IconPath;
        public string MenuResolver;
        public long keyCode;
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(InteractMenuConfig), "items")]
    public class InteractMenuConfigCollection
    {
        private readonly Dictionary<long, InteractMenuConfig> lookup = new ();
        private readonly List<InteractMenuConfig> items = new ();
        private static InteractMenuConfigCollection _instance;
        private InteractMenuConfigCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(var evt in items) {
                lookup.Add(evt.ID, evt);
            }
        }

        public InteractMenuConfig GetConfig(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public static InteractMenuConfigCollection Instance => _instance ??= new InteractMenuConfigCollection();
        
        
    }
}