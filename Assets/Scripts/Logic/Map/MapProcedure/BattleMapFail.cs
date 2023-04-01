using System;

namespace Logic.Map.MapProcedure
{
    public class BattleMapFail : BattleMapProcedure
    {
        private Action _onFinish;
        public override void StartProcedure(BattlePlaceController from)
        {
            var result = BattleResultPanel.Show("战斗失败", _onFinish);
        }

        public override void OnProcedureFinish(Action onFinish)
        {
            _onFinish += onFinish;
        }

        public override void TerminateProcedure()
        {
            throw new NotImplementedException();
        }
    }
}