using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    public class ChatBroadcaster
    {
        public MessageType MessageType { get; }

        public DateTime LastMessage = DateTime.MinValue;

        public static readonly HashSet<MessageType> AcceptedTypes = new HashSet<MessageType> { MessageType.Shout, MessageType.Yell, MessageType.Say, MessageType.FreeCompany, MessageType.Echo };

        public int MinDelayMs { get; }

        public ChatBroadcaster(MessageType messageType = MessageType.Shout, int minDelayMs = 1000)
        {
            MessageType = messageType;

            MinDelayMs = minDelayMs;
        }

        public async Task Send(string message)
        {
            if ((DateTime.Now - LastMessage).TotalMilliseconds < MinDelayMs)
            {
                await Coroutine.Sleep((int)(MinDelayMs - (DateTime.Now - LastMessage).TotalMilliseconds));
            }

            switch (MessageType)
            {
                case MessageType.FreeCompany:
                    ChatManager.SendChat("/fc " + message);
                    break;
                case MessageType.Say:
                    ChatManager.SendChat("/say " + message);
                    break;
                case MessageType.Shout:
                    ChatManager.SendChat("/shout " + message);
                    break;
                case MessageType.Yell:
                    ChatManager.SendChat("/yell " + message);
                    break;
                case MessageType.Echo:
                    ChatManager.SendChat("/echo " + message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LastMessage = DateTime.Now;
        }
    }
}