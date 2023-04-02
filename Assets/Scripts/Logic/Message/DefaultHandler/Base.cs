using System;

namespace Logic.Message.DefaultHandler
{

    [AttributeUsage(AttributeTargets.Class)]
    public class MessageDefaultHandler : Attribute {
        public Type MessageType {get; private set;}
        
        public MessageDefaultHandler(Type messageType) {
            MessageType = messageType;
        }
    }

    public interface IMessageHandler
    {
        public void Enable();

        public void Disable();
    }
}