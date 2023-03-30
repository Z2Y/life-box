using System;
using System.Collections.Generic;
using Controller;
using MessagePack;
using Model;
using Realms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public partial class Quest : IRealmObject
    {
        [PrimaryKey]
        public long ID { get; set; }
        public QuestType QuestType => (QuestType)IQuestType;
        public int IQuestType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Award { get; set; }
        public string StartEffect { get; set; }
        public string TerminateEffect { get; set; }
        public string CompleteEffect { get; set; }
        public long StartEventID { get; set; }
        public IList<long> EndEventIDs { get;  }
    }
    
    public enum QuestType {
        Main = 0,
        Bounty = 1,
        Rumor = 2
    }
}

namespace ModelContainer
{
    public static class QuestCollection
    {
        public static Quest GetQuest(long id)
        {
            return RealmDBController.Db.Find<Quest>(id);
        }
    }
}