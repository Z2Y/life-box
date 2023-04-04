using Logic.Quest;
using Model;
using StructLinq;
using UnityEngine.EventSystems;

namespace UI
{
    public class QuestTypeFilter: UIBase, IPointerClickHandler
    {
        public QuestType QuestType;
        public QuestPanel questPanel;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (questPanel != null && questPanel.activeQuestType != QuestType) {
                questPanel.ShowQuestList(QuestType);
                questPanel.ShowQuestDetailPanel(QuestManager.Instance.GetQuestByType(QuestType).ToStructEnumerable().FirstOrDefault());
            }
        }
    }
}