using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Model;
using MessagePack;
using ModelContainer;
using MongoDB.Bson;
using Realms;
using UnityEngine;

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

        public List<Event> GetValidEvents()
        {
            return EventCollection.GetValidEvents(Event).ToList();
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
