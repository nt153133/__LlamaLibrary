using System;
using System.Collections.Generic;
using ff14bot.Enums;

namespace LlamaLibrary.Helpers.MessageHandling
{
    public abstract class ChatReceiver<T> : IChatReceiver
    {
        public virtual IEnumerable<MessageType> TypesToReceive { get; } = new List<MessageType>();

        public virtual T ParseMessage(string message, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessMessage(T message)
        {
            throw new NotImplementedException();
        }

        public void ProcessMessage(string message, DateTime timeStamp)
        {
            ProcessMessage(ParseMessage(message, timeStamp));
        }
    }

    public interface IChatReceiver
    {
        IEnumerable<MessageType> TypesToReceive { get; }
        void ProcessMessage(string message, DateTime timeStamp);
    }
}