using System;
using Logic.Map;
using UniTaskPubSub;

namespace Logic.Message.DefaultHandler
{
    [MessageDefaultHandler(typeof(BattleComplete))]
    public class BattleCompleteHandler : IMessageHandler
    {
        private IDisposable _disposable;
        
        public void Enable()
        {
            if (_disposable != null) return;
            _disposable = AsyncMessageBus.Default.Subscribe<BattleComplete>(Handle);
        }

        public void Disable()
        {
            if (_disposable == null) return;
            _disposable.Dispose();
            _disposable = null;
        }

        private void Handle(PlaceMessage message)
        {
            if (message is not BattleComplete) return;
            var prevNode = LifeEngine.Instance.lifeData.current.Prev;
            
            if (prevNode != null)
            {
                MapGate.JumpTo(prevNode.Location.MapID, prevNode.Location.Position);
            }
        }
    }
}