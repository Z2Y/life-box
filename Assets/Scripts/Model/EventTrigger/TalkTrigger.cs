using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{
    [MessagePackObject(true)]
    public class TalkTrigger : IEventTrigger
    {
        public long ID;
        public long RelationLimit;
        public long[] Event;
        public float[] Weight;

        public Event GetEvent()
        {
            return ModelContainer.EventCollection.RandomEvent(Event, Weight);
        }
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(TalkTrigger), "triggers")]
    public class TalkTriggerContainer
    {
        private Dictionary<long, TalkTrigger> lookup = new Dictionary<long, TalkTrigger>();
        private List<TalkTrigger> triggers = new List<TalkTrigger>();
        private static TalkTriggerContainer _instance;
        private TalkTriggerContainer() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (var trigger in triggers)
            {
                lookup.Add(trigger.ID, trigger);
            }
        }

        public static TalkTriggerContainer Instance => _instance ?? (_instance = new TalkTriggerContainer());

        public TalkTrigger GetTrigger(long characterID)
        {
            return lookup.TryGetValue(characterID, out var value) ? value : null;
        }
    }
}