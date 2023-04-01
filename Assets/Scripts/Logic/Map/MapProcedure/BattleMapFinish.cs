using System;
using UI;

namespace Logic.Map.MapProcedure
{
    public class BattleMapFinish : BattleMapProcedure
    {
        private Action _onFinish;
        public override void StartProcedure(BattlePlaceController from)
        {
            var result = BattleResultPanel.Show("战斗结束", _onFinish);
        }

        public override void OnProcedureFinish(Action onFinish)
        {
            _onFinish += onFinish;
            UIManager.Instance.FindByType<BattleResultPanel>()?.Hide();
        }

        public override void TerminateProcedure()
        {
            UIManager.Instance.FindByType<BattleResultPanel>()?.Hide();
        }
    }
}