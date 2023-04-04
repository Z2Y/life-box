using System;
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

        public IEnumerable<Event> GetTalks()
        {
            return EventCollection.GetValidEvents(Event);
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