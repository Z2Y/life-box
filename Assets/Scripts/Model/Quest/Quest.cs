using System;
using System.Collections.Generic;
using MessagePack;
using Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public class Quest
    {
        public long ID;
        public QuestType QuestType;
        public string Title;
        public string Description;
        public string Requirement;
        public string Award;
        public string StartEffect;
        public string TerminateEffect;
        public string CompleteEffect;
        public long StartEventID;
        public long[] EndEventIDs;
    }
    
    public enum QuestType {
        Main = 0,
        Bounty = 1,
        Rumor = 2
    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Quest), "quests")]
    public class QuestCollection
    {
        private readonly Dictionary<long, Quest> lookup = new ();
        private readonly List<Quest> quests = new ();
        private static QuestCollection _instance;
        public static QuestCollection Instance => _instance ??= new QuestCollection();

        private void OnLoad() {
            lookup.Clear();
            foreach(var quest in quests) {
                lookup.Add(quest.ID, quest);
            }
        }
        
        public Quest GetQuest(long id)
        {
            return lookup.TryGetValue(id, out var value) ? value : null;
        }
    }
}