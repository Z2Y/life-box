using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using MessagePack;
using ModelContainer;
using Realms;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public partial class PlaceTrigger : IRealmObject

    {
    public long ID { get; set; }
    public IList<long> Event { get; }
    public IList<long> Priority { get;  }
    public IList<string> Stage { get; }

    public List<Event> GetValidEvents()
    {
        return EventCollection.GetValidEvents(Event).ToList();
    }
    }
}
namespace ModelContainer
{
    [ModelContainerOf(typeof(PlaceTrigger), "triggers")]
    public class PlaceTriggerContainer
    {
        private readonly Dictionary<long, PlaceTrigger> lookup = new ();
        private readonly List<PlaceTrigger> triggers = new ();
        private static PlaceTriggerContainer _instance;
        private PlaceTriggerContainer() { }

        private void OnLoad()
        {
            lookup.Clear();
            foreach (var trigger in triggers)
            {
                lookup.Add(trigger.ID, trigger);
            }
        }

        public static PlaceTriggerContainer Instance => _instance ??= new PlaceTriggerContainer();

        public PlaceTrigger GetPlaceTrigger(long placeID)
        {
            return lookup.TryGetValue(placeID, out var value) ? value : null;
        }
    }
}
