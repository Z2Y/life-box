using System;
using FancyScrollView;
using Model;
using UnityEngine;

namespace UI
{
    public class QuestListView : FancyGridView<Quest, QuestCellContext>
    {
        class CellGroup : DefaultCellGroup { }
        
        [SerializeField] QuestListCell cellPrefab = default;
        protected override void SetupCellTemplate() => Setup<CellGroup>(cellPrefab);
        
        public void OnPointerClickCell(Action<Quest> callback)
        {
            Context.OnPointerClickCell = callback;
        }
    }
    
    public class QuestCellContext : FancyGridViewContext
    {
        public long activeQuestID;
        public Action<int> OnPointerEnterCell;
        public Action<int> OnPointerExitCell;
        public Action<Quest> OnPointerClickCell;
    }
}