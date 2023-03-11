using Assets.HeroEditor.Common.Scripts.Common;
using Model;
using UnityEngine.UI;

namespace UI
{
    public class QuestDetailPanel : UIBase
    {
        public Text questTitleText;
        public Text questDescriptionText;
        public Text questRequirementText;
        
        public void ShowQuestDetail(Quest quest)
        {
            // 显示任务详情面板
            Show();

            if (quest == null)
            {
                ShowEmpty();
                return;
            }

            // 在任务详情面板中显示任务信息
            questTitleText.text = quest.Title;
            questDescriptionText.text = quest.Description;

            questRequirementText.text = quest.Requirement;
            questRequirementText.gameObject.SetActive(!questRequirementText.text.IsEmpty());
        }

        private void ShowEmpty()
        {
            questTitleText.text = "还没有开始任何任务";
            questDescriptionText.text = "";
            questRequirementText.text = "";
        }
    }
}