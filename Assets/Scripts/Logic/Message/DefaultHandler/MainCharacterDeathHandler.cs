using System;
using UniTaskPubSub;

namespace Logic.Message.DefaultHandler
{
    [MessageDefaultHandler(typeof(CharacterDeath))]
    public class MainCharacterDeathHandler : IMessageHandler
    {
        private IDisposable _disposable;

        public void Enable()
        {
            if (_disposable != null) return;
            _disposable = AsyncMessageBus.Default.Subscribe<CharacterDeath>(Handle);
        }

        public void Disable()
        {
            if (_disposable == null) return;
            _disposable.Dispose();
            _disposable = null;
        }

        private void Handle(CharacterDeath message)
        {
            if (message.characterID != 0) return;

            LifeEngine.Instance.GameEnd();
        }
    }
}