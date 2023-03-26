using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Model;

namespace Logic.Map.MapProcedure
{
    public class BattleMapEvent : BattleMapProcedure
    {

        public Event mapEvent;

        private Action _onFinish;

        private CancellationTokenSource cancelTokenSource;
        public override void StartProcedure(BattlePlaceController place)
        {
            cancelTokenSource = new CancellationTokenSource();
            mapEvent.Trigger().AttachExternalCancellation(cancelTokenSource.Token).ContinueWith(finish).Coroutine();
        }

        private void finish(EventNode node)
        {
            _onFinish?.Invoke();
        }

        public override void OnProcedureFinish(Action onFinish)
        {
            _onFinish += onFinish;
        }

        public override void TerminateProcedure()
        {
            cancelTokenSource.Cancel();
        }
    }
}