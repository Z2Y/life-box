using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using Cysharp.Threading.Tasks;
using MessagePack;
using Model;
using Realms;

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
    public partial class Event : IEventTrigger, IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public int EventType { get; set; }
        public string Name { get; set; }
        public long Unique { get; set; }
        public string Description { get; set; }
        public string Effect { get; set; }
        public string BranchExpression { get; set; }
        public IList<long> Branch { get; }
        public string Include { get; set; }
        public string Exclude { get; set; }
        
        public Event GetEvent()
        {
            return this;
        }
    }
}

namespace ModelContainer
{
    public static class EventCollection
    {
        public static Event GetEvent(long id)
        {
            return RealmDBController.Db.Find<Event>(id);
        }

        public static IEnumerable<Event> GetEventByType(EventType eventType)
        {
            return RealmDBController.Db.All<Event>().Where((evt) => evt.EventType == (int)eventType);
        }

        private static IEnumerable<int> GetValidEventIndex(IEnumerable<long> events) {
            return events.Select((id, idx) =>
            {
                var e = GetEvent(id);
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

        public static IEnumerable<Event> GetValidEvents(IEnumerable<long> events)
        {
            return events.Select((id) =>
            {
                var e = GetEvent(id);
                if (e == null) return null;
                if (e.Exclude.ExecuteExpression() is true) return null;
                if (e.Include.ExecuteExpression() is bool isInclude)
                {
                    // UnityEngine.Debug.Log($"isInclude {(bool?)isInclude}");
                    return isInclude ? e : null;
                }
                return e;
            }).Where((e) => e != null);
        }

        public static int RandomEventIndex(IEnumerable<long> events, IList<float> weights) {
            var validEvents = GetValidEventIndex(events).ToList();
            if (validEvents.Count == 0) { return -1; }
            var targetW = UnityEngine.Random.Range(0, validEvents.Select((int idx) => weights[idx]).Sum());
            float currentW = 0;
            return validEvents.FirstOrDefault((int idx) => {
                currentW += weights[idx];
                return currentW > targetW;
            });
        }

        public static Event RandomEvent(IList<long> events, IList<float> weights) {
            var index = RandomEventIndex(events, weights);
            return index < 0 ? null : GetEvent(events[index]);
        }
    }
}