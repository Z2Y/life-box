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
        private Dictionary<long, Model.TalkTrigger> lookup = new Dictionary<long, Model.TalkTrigger>();
        private List<Model.TalkTrigger> triggers = new List<Model.TalkTrigger>();
        private static TalkTriggerContainer _instance;
        private TalkTriggerContainer() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (Model.TalkTrigger trigger in triggers)
            {
                lookup.Add(trigger.ID, trigger);
            }
        }

        public static TalkTriggerContainer Instance => _instance ?? (_instance = new TalkTriggerContainer());

        public Model.TalkTrigger GetTrigger(long characterID)
        {
            return lookup.TryGetValue(characterID, out var value) ? value : null;
        }
    }
}