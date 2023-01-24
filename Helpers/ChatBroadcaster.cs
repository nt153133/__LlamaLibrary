using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Extensions;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public class ChatBroadcaster
    {
        public static readonly HashSet<MessageType> AcceptedTypes = new() { MessageType.Shout, MessageType.Yell, MessageType.Say, MessageType.FreeCompany, MessageType.Echo, MessageType.CustomEmotes, MessageType.StandardEmotes };

        public static DateTime LastPerson = DateTime.MinValue;

        public DateTime LastMessage = DateTime.MinValue;

        public ChatBroadcaster(MessageType messageType = MessageType.Shout, int minDelayMs = 1000)
        {
            MessageType = messageType;

            MinDelayMs = minDelayMs;
        }

        public MessageType MessageType { get; set; }

        public int MinDelayMs { get; set; }

        public static int MinDelayTellMs { get; set; } = 2000;

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
                case MessageType.CWLS:
                    ChatManager.SendChat("/cwlinkshell1 " + message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LastMessage = DateTime.Now;
        }

        public async Task Send(byte[] message)
        {
            if ((DateTime.Now - LastMessage).TotalMilliseconds < MinDelayMs)
            {
                await Coroutine.Sleep((int)(MinDelayMs - (DateTime.Now - LastMessage).TotalMilliseconds));
            }

            var messageByte = new List<byte>();
            switch (MessageType)
            {
                case MessageType.FreeCompany:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/fc "));
                    break;
                case MessageType.Say:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/say "));
                    break;
                case MessageType.Shout:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/shout "));
                    break;
                case MessageType.Party:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/p "));
                    break;
                case MessageType.Yell:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/yell "));
                    break;
                case MessageType.Echo:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/echo "));
                    break;
                case MessageType.CustomEmotes:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/em "));
                    break;
                case MessageType.StandardEmotes:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/"));
                    break;
                case MessageType.CWLS:
                    messageByte.AddRange(Encoding.UTF8.GetBytes("/cwlinkshell1 "));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            messageByte.AddRange(message);
            SendChat(messageByte.ToArray());

            LastMessage = DateTime.Now;
        }

        public async Task TellPlayer(string playerName, string message)
        {
            var character = GameObjectManager.GameObjects.FirstOrDefault(i => i.Name.Contains(playerName));

            if (character != null && character.Type == GameObjectType.Pc)
            {
                var target = GameObjectManager.GetObjectById<Character>(character.ObjectId, true) as Character;
                await SendTell(target, message);
            }
        }

        public async Task<bool> SendTell(Character? character, string message)
        {
            if (character == null || character.Type != GameObjectType.Pc)
            {
                return false;
            }

            if ((DateTime.Now - LastMessage).TotalMilliseconds < MinDelayMs)
            {
                await Coroutine.Sleep((int)(MinDelayMs - (DateTime.Now - LastMessage).TotalMilliseconds));
            }

            if ((DateTime.Now - LastPerson).TotalMilliseconds < MinDelayTellMs)
            {
                await Coroutine.Sleep((int)(MinDelayTellMs - (DateTime.Now - LastPerson).TotalMilliseconds));
            }

            ChatManager.SendChat($"/t {character.Name}@{character.HomeWorld()} {message}");

            LastPerson = DateTime.Now;

            return true;
        }

        public async Task<bool> SendTellToTarget(string message)
        {
            if (!Core.Me.HasTarget || GameObjectManager.Target.Type != GameObjectType.Pc)
            {
                return false;
            }

            return await SendTell(GameObjectManager.GetObjectById<Character>(GameObjectManager.Target.ObjectId, true) as Character, message);
        }

        public void SetDelay(int ms)
        {
            MinDelayMs = ms;
        }

        public async Task SendMessage(string line)
        {
            await Send(line);
        }

        public async Task SendMessage(string line, MessageType messageType)
        {
            var oldType = MessageType;
            SetType(messageType);
            await Send(line);
            SetType(oldType);
        }

        public void SetType(MessageType messageType)
        {
            MessageType = messageType;
        }

        public static void SendChat(byte[] array)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                using var allocatedMemory2 = Core.Memory.CreateAllocatedMemory(400);
                using var allocatedMemory = Core.Memory.CreateAllocatedMemory(array.Length + 30);
                allocatedMemory.AllocateOfChunk("start", array.Length);
                allocatedMemory.WriteBytes("start", array);
                allocatedMemory2.AllocateOfChunk<IntPtr>("dword0");
                allocatedMemory2.AllocateOfChunk<long>("dword4");
                allocatedMemory2.AllocateOfChunk<long>("dword8");
                allocatedMemory2.AllocateOfChunk<long>("dwordC");
                allocatedMemory2.Write("dword0", allocatedMemory.Address);
                allocatedMemory2.Write("dword4", 64);
                allocatedMemory2.Write("dword8", array.Length + 1);
                allocatedMemory2.Write("dwordC", 0);
                Core.Memory.CallInjected64<int>(Offsets.ExecuteCommandInner,
                                                UiManagerProxy.RaptureShellModule,
                                                allocatedMemory2.Address,
                                                UiManagerProxy.UIModule);
            }
        }
    }
}