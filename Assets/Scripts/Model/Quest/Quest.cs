using System;
using System.Collections.Generic;
using MessagePack;
using Model;

namespace Model
{
    [MessagePackObject(true)]
    [Serializable]
    public class Quest
    {
        public long ID;
        public string title;
        public string description;
        public long startEventID;
        public long endEventIDs;

    }
}

namespace ModelContainer
{
    [ModelContainerOf(typeof(Quest), "quests")]
    public class QuestCollection: Singleton<QuestCollection>
    {
        private readonly Dictionary<long, Quest> lookup = new ();
        private List<Quest> quests = new ();

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