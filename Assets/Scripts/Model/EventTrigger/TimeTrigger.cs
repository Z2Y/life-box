using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{
    [MessagePackObject(true)]
    public class TimeTrigger : IEventTrigger
    {
        public long ID;
        public int Year;
        public int Month;
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
    [ModelContainerOf(typeof(Model.TimeTrigger), "triggers")]
    public class TimeTriggerContainer
    {
        private Dictionary<TimeSpan, Model.TimeTrigger> lookup = new Dictionary<TimeSpan, Model.TimeTrigger>();
        private List<Model.TimeTrigger> triggers = new List<Model.TimeTrigger>();
        private static TimeTriggerContainer _instance;
        private TimeTriggerContainer() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (Model.TimeTrigger trigger in triggers)
            {
                lookup.Add(new TimeSpan(trigger.Year, trigger.Month), trigger);
            }
        }

        public static TimeTriggerContainer Instance => _instance ?? (_instance = new TimeTriggerContainer());

        public Model.TimeTrigger GetTrigger(TimeSpan timeSpan)
        {
            if (lookup.TryGetValue(timeSpan, out var value))
            {
                return value;
            }

            if (lookup.TryGetValue(new TimeSpan(-1, timeSpan.Month), out var monthly))
            {
                return monthly;
            }
            return null;
        }
    }
}