using System.Threading.Tasks;
using Logic.Quest;
using Model;
using StructLinq;
using UnityEngine;
using Utils;

namespace UI
{
    [PrefabResource("Prefabs/ui/QuestPanel")]
    public class QuestPanel : UIBase
    {

        // 类别列表和内容列表的内容容器
        public RectTransform categoryListContent;

        public QuestListView questListView;

        // 任务详情面板和返回按钮
        public QuestDetailPanel questDetailPanel;
        
        public QuestType activeQuestType { get; private set; }

        private void Awake()
        {
            transform.SetParent(UIManager.Instance.worldRoot);
        }

        void Start()
        {
            // 初始化任务数据列表
            InitQuestList();

            // 在类别列表中创建类别按钮
            ShowMainQuest();

            // 注册返回按钮的点击事件
            closeBtn.onClick.AddListener(Hide);
        }

        // 初始化任务数据列表
        void InitQuestList()
        {
            questListView.OnPointerClickCell(ShowQuestDetailPanel);
        }

        // 在类别列表中创建类别按钮
        void ShowMainQuest()
        {
            // 默认选中第一个任务类型的按钮
            ShowQuestList(QuestType.Main);
            ShowQuestDetailPanel(QuestManager.Instance.GetQuestByType(QuestType.Main).ToStructEnumerable().FirstOrDefault());
        }

        // 显示对应任务类型的任务列表
        public void ShowQuestList(QuestType questType)
        {
            // 获取对应任务类型的任务列表
            var quests = QuestManager.Instance.GetQuestByType(questType).ToStructEnumerable().ToArray();
            activeQuestType = questType;
            questListView.UpdateContents(quests);
            Show();
        }

        public void ShowQuestDetailPanel(Quest quest)
        {
            // 显示任务详情面板
            questDetailPanel.ShowQuestDetail(quest);
        }

        public static async Task<QuestPanel> ShowPanel()
        {
            var panel = await UIManager.Instance.FindOrCreateAsync<QuestPanel>(true) as QuestPanel;
            if (!ReferenceEquals(panel, null))
            {
                panel.ShowMainQuest();
            }

            return panel;
        }
    }
}