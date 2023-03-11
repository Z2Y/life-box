using FancyScrollView;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class QuestListCell : FancyGridViewCell<Quest, QuestCellContext>, IPointerClickHandler
    {
        [SerializeField] private Text title;
        
        private Quest quest;
        public override void UpdateContent(Quest itemData)
        {
            quest = itemData;
            title.text = itemData.Title;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Context.OnPointerClickCell?.Invoke(quest);
            Context.activeQuestID = quest.ID;
        }
    }
}