using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using StructLinq;
using UnityEngine;

namespace Logic.Quest
{
    public class QuestManager : Singleton<QuestManager>
    {
        private readonly HashSet<long> activeQuest = new ();
        private readonly HashSet<long> completedQuest = new();
        private readonly HashSet<long> terminatedQuest = new();
        private readonly Dictionary<QuestType, HashSet<long>> questsByType = new ();

        public IEnumerable<Model.Quest> GetQuestByType(QuestType questType)
        {
            if (!questsByType.ContainsKey(questType))
            {
                return StructEnumerable.Empty<Model.Quest>().ToEnumerable();
            }

            return questsByType[questType].ToStructEnumerable().Select(QuestCollection.GetQuest).ToEnumerable();
        }

        public async Task<object> AddQuest(Model.Quest quest)
        {
            if (isStarted(quest.ID))
            {
                return null;
            }

            activeQuest.Add(quest.ID);
            if (questsByType.ContainsKey(quest.QuestType))
            {
                questsByType[quest.QuestType].Add(quest.ID);
            }
            else
            {
                questsByType[quest.QuestType] = new HashSet<long> { quest.ID };
            }
            Debug.Log(quest.StartEffect);
            return await quest.StartEffect.ExecuteExpressionAsync();
        }

        public async Task<object> CompleteQuest(Model.Quest quest, bool allowRestart = false)
        {
            if (!isActive(quest.ID)) return null;
            activeQuest.Remove(quest.ID);
            questsByType[quest.QuestType]?.Remove(quest.ID);
            if (!allowRestart)
            {
                completedQuest.Add(quest.ID);
            }
            
            return await quest.CompleteEffect.ExecuteExpressionAsync();
        }

        public async Task<object> TerminateQuest(Model.Quest quest, bool allowRestart = false)
        {
            if (!isActive(quest.ID)) return null;
            activeQuest.Remove(quest.ID);
            questsByType[quest.QuestType]?.Remove(quest.ID);
            if (!allowRestart)
            {
                terminatedQuest.Add(quest.ID);
            }
            
            return await quest.TerminateEffect.ExecuteExpressionAsync();
        }

        public bool isActive(long questID)
        {
            return activeQuest.Contains(questID);
        }

        public bool isCompleted(long questID)
        {
            return completedQuest.Contains(questID);
        }

        public bool isTerminated(long questID)
        {
            return terminatedQuest.Contains(questID);
        }

        public bool isStarted(long questID)
        {
            return isActive(questID) || isTerminated(questID) || isTerminated(questID);
        }
    }
}