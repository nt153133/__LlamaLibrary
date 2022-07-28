using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Helpers
{
    public class ChatBroadcaster
    {
        public MessageType MessageType { get; set; }

        public DateTime LastMessage = DateTime.MinValue;

        public static readonly HashSet<MessageType> AcceptedTypes = new HashSet<MessageType> { MessageType.Shout, MessageType.Yell, MessageType.Say, MessageType.FreeCompany, MessageType.Echo, MessageType.CustomEmotes, MessageType.StandardEmotes };

        public int MinDelayMs { get; set; }

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
                case MessageType.Party:
                    ChatManager.SendChat("/p " + message);
                    break;
                case MessageType.Yell:
                    ChatManager.SendChat("/yell " + message);
                    break;
                case MessageType.Echo:
                    ChatManager.SendChat("/echo " + message);
                    break;
                case MessageType.CustomEmotes:
                    ChatManager.SendChat("/em " + message);
                    break;
                case MessageType.StandardEmotes:
                    ChatManager.SendChat("/" + message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LastMessage = DateTime.Now;
        }

        public static async Task<bool> SendTell(Character character, string message)
        {
            if (character == null || character.Type != GameObjectType.Pc)
            {
                return false;
            }

            ChatManager.SendChat($"/t {character.Name}@{character.HomeWorld()} {message}");

            return true;
        }

        public static async Task<bool> SendTellToTarget(string message)
        {
            if (!Core.Me.HasTarget || GameObjectManager.Target.Type != GameObjectType.Pc)
            {
                return false;
            }

            return await SendTell(GameObjectManager.GetObjectById<Character>(GameObjectManager.Target.ObjectId, true) as Character, message);
        }
    }
}