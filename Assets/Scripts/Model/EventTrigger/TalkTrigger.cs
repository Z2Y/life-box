using System;
using System.Linq;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using ModelContainer;
using Realms;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public partial class TalkTrigger : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public long RelationLimit { get; set; }
        public IList<long> Event { get;  }
        public IList<float> Weight { get; }

        public Event GetEvent()
        {
            return EventCollection.RandomEvent(Event, Weight);
        }

        public List<Event> GetTalks()
        {
            return EventCollection.GetValidEvents(Event).ToList();
        }
    }
}

namespace ModelContainer
{
    public static class TalkTriggerContainer
    {
        public static TalkTrigger GetTalkConfig(long characterID)
        {
            return RealmDBController.Db.Find<TalkTrigger>(characterID);
        }
    }
}