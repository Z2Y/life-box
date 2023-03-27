using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using Model;

public abstract class GameEvent
{
    public long id;
    public string name;
    public string description;
}

namespace Model
{

    public enum EventType {
        Born = 0,
        Death = 1,
        Normal = 2,
        Route = 3,
        Shop = 4,
        Talk = 5
    }

    [MessagePackObject(true)]
    [Serializable]
    public class Event : IEventTrigger
    {
        public long ID;
        public EventType EventType;
        public string Name;
        public long Unique;
        public string Description;
        public string Effect;
        public string BranchExpression;
        public long[] Branch;
        public string Include;
        public string Exclude;
        
        public Event GetEvent()
        {
            return this;
        }
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Event), "events")]
    public class EventCollection
    {
        private readonly Dictionary<long, Event> lookup = new ();
        private List<Event> events = new ();
        private static EventCollection _instance;
        private EventCollection() { }

        private void OnLoad() {
            lookup.Clear();
            foreach(Event evt in events) {
                lookup.Add(evt.ID, evt);
            }
        }

        public Event GetEvent(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }

        public IEnumerable<Model.Event> GetEventByType(Model.EventType eventType)
        {
            return events.Where((evt) => evt.EventType == eventType);
        }

        public static EventCollection Instance => _instance ??= new EventCollection();

        public static IEnumerable<int> GetValidEventIndex(IEnumerable<long> events) {
            return events.Select((id, idx) =>
            {
                var e = Instance.GetEvent(id);
                if (e == null) return -1;
                if (e.Exclude.ExecuteExpression() is true) return -1;
                if (e.Include.ExecuteExpression() is bool isInclude)
                {
                    // UnityEngine.Debug.Log($"isInclude {(bool?)isInclude}");
                    return isInclude ? idx : -1;
                }
                return idx;
            }).Where((int v) => v >= 0);            
        }

        public static IEnumerable<Model.Event> GetValidEvents(long[] events) {
            return GetValidEventIndex(events).Select((idx) => Instance.GetEvent(events[idx]));
        }

        public static int RandomEventIndex(IEnumerable<long> events, float[] weights) {
            var validEvents = GetValidEventIndex(events).ToList();
            if (validEvents.Count == 0) { return -1; }
            var targetW = UnityEngine.Random.Range(0, validEvents.Select((int idx) => weights[idx]).Sum());
            float currentW = 0;
            return validEvents.FirstOrDefault((int idx) => {
                currentW += weights[idx];
                return currentW > targetW;
            });
        }

        public static Event RandomEvent(long[] events, float[] weights) {
            int index = RandomEventIndex(events, weights);
            if (index < 0) return null;
            return Instance.GetEvent(events[index]);
        }
    }
}