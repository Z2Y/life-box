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
    }

    [MessagePackObject(true)]
    public class Event
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
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Event), "events")]
    public class EventCollection
    {
        private Dictionary<long, Event> lookup = new Dictionary<long, Event>();
        private List<Event> events = new List<Event>();
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

        public static EventCollection Instance => _instance ?? (_instance = new EventCollection());

        public static IEnumerable<int> GetValidEventIndex(IEnumerable<long> events) {
            return events.Select((long id, int idx) =>
            {
                Model.Event e = Instance.GetEvent(id);
                if (e == null) return -1;
                var isExclude = e.Exclude.ExecuteExpression() as bool?;
                if (isExclude != null && (bool)isExclude) return -1;
                var isInclude = e.Include.ExecuteExpression() as bool?;
                if (isInclude != null)
                {
                    UnityEngine.Debug.Log($"isInclude {isInclude}");
                    return (bool)isInclude ? idx : -1;
                }
                return idx;
            }).Where((int v) => v >= 0);            
        }

        public static IEnumerable<Model.Event> GetValidEvents(long[] events) {
            return GetValidEventIndex(events).Select((idx) => Instance.GetEvent(events[idx]));
        }

        public static int RandomEventIndex(long[] events, float[] weights) {
            List<int> validEvents = GetValidEventIndex(events).ToList();
            if (validEvents.Count == 0) { return -1; }
            float targetW = UnityEngine.Random.Range(0, validEvents.Select((int idx) => weights[idx]).Sum());
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