using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
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
        [PrimaryKey]
        public long ID { get; set; }
        public IList<long> Event { get; }
        public IList<long> Priority { get;  }
        public IList<string> Stage { get; }

        public  IEnumerable<Event> GetValidEvents()
        {
            return EventCollection.GetValidEvents(Event);
        }

        public  IEnumerable<Event> GetValidEventsOfState(params string[] stages)
        {
            
            var validIndex = Stage.Select((stage, idx) => stages.Contains(stage) ? idx : -1).Where((idx) => idx >= 0);

            return EventCollection.GetValidEvents(Event.Where((eventID, idx) => validIndex.Contains(idx)));
        }
    }
}
namespace ModelContainer
{
    public static class PlaceTriggerContainer
    {
        public static PlaceTrigger GetPlaceTrigger(long placeID)
        {
            return RealmDBController.Db.Find<PlaceTrigger>(placeID);
        }
    }
}
