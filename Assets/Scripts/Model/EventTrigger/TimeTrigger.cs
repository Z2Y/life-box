using System;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using Realms;
using StructLinq;

namespace Model
{
    [Serializable]
    [MessagePackObject(true)]
    public partial class TimeTrigger : IEventTrigger, IRealmObject
    {
        public long ID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public IList<long> Event { get; }
        public IList<float> Weight { get; }

        public Event GetEvent()
        {
            return ModelContainer.EventCollection.RandomEvent(Event, Weight);
        }
    }
}

namespace ModelContainer
{
    public static class TimeTriggerContainer
    {
        public static TimeTrigger GetTrigger(TimeSpan timeSpan)
        {
            return RealmDBController.Db.All<TimeTrigger>().Filter($"Year == {timeSpan.Year} && Month == {timeSpan.Month}").ToStructEnumerable().FirstOrDefault();
        }
    }
}