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
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Sends rate-limited chat messages in a configurable FFXIV channel.
    /// Supports text and emote channels and provides helpers for player tells.
    /// </summary>
    public class ChatBroadcaster
    {
        /// <summary>The set of message types that can be sent through this broadcaster.</summary>
        public static readonly HashSet<MessageType> AcceptedTypes = new() { MessageType.Shout, MessageType.Yell, MessageType.Say, MessageType.FreeCompany, MessageType.Echo, MessageType.CustomEmotes, MessageType.StandardEmotes };

        /// <summary>The UTC timestamp of the most recent /tell sent to any player (shared across all instances).</summary>
        public static DateTime LastPerson = DateTime.MinValue;

        /// <summary>The UTC timestamp of the most recent message sent by this broadcaster instance.</summary>
        public DateTime LastMessage = DateTime.MinValue;

        /// <summary>
        /// Initializes a new <see cref="ChatBroadcaster"/> with the specified channel and rate-limit delay.
        /// </summary>
        /// <param name="messageType">The FFXIV chat channel to use for outgoing messages.</param>
        /// <param name="minDelayMs">Minimum milliseconds between messages sent by this instance.</param>
        public ChatBroadcaster(MessageType messageType = MessageType.Shout, int minDelayMs = 1000)
        {
            MessageType = messageType;

            MinDelayMs = minDelayMs;
        }

        /// <summary>Gets or sets the FFXIV chat channel used by this broadcaster.</summary>
        public MessageType MessageType { get; set; }

        /// <summary>Gets or sets the minimum delay in milliseconds between consecutive messages sent by this instance.</summary>
        public int MinDelayMs { get; set; }

        /// <summary>Gets or sets the minimum delay in milliseconds between /tell messages (shared across all instances).</summary>
        public static int MinDelayTellMs { get; set; } = 2000;

        /// <summary>
        /// Sends a text message to the configured chat channel, waiting if the rate-limit has not yet elapsed.
        /// </summary>
        /// <param name="message">The text to send.</param>
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

        /// <summary>
        /// Sends a pre-encoded byte message to the configured chat channel.
        /// Useful for messages containing FFXIV special-character payloads.
        /// </summary>
        /// <param name="message">The UTF-8 encoded message bytes to send (without channel prefix).</param>
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

        /// <summary>
        /// Searches visible game objects for a player with a matching name and sends them a /tell.
        /// </summary>
        /// <param name="playerName">Partial or full character name to search for.</param>
        /// <param name="message">The message to send.</param>
        public async Task TellPlayer(string playerName, string message)
        {
            var character = GameObjectManager.GameObjects.FirstOrDefault(i => i.Name.Contains(playerName));

            if (character != null && character.Type == GameObjectType.Pc)
            {
                var target = GameObjectManager.GetObjectById<Character>(character.ObjectId, true);
                await SendTell(target, message);
            }
        }

        /// <summary>
        /// Sends a /tell to a specific <see cref="Character"/> object, respecting both per-message and per-person rate limits.
        /// </summary>
        /// <param name="character">The player character to send the tell to.</param>
        /// <param name="message">The message text to send.</param>
        /// <returns><see langword="true"/> if the tell was sent; <see langword="false"/> if the character is invalid.</returns>
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

        /// <summary>
        /// Sends a /tell to the player's current target, if the target is a player character.
        /// </summary>
        /// <param name="message">The message text to send.</param>
        /// <returns><see langword="true"/> if the tell was sent; <see langword="false"/> if no valid PC target is selected.</returns>
        public async Task<bool> SendTellToTarget(string message)
        {
            if (!Core.Me.HasTarget || GameObjectManager.Target.Type != GameObjectType.Pc)
            {
                return false;
            }

            return await SendTell(GameObjectManager.GetObjectById<Character>(GameObjectManager.Target.ObjectId, true), message);
        }

        /// <summary>Updates the minimum inter-message delay for this instance.</summary>
        /// <param name="ms">New delay in milliseconds.</param>
        public void SetDelay(int ms)
        {
            MinDelayMs = ms;
        }

        /// <summary>Sends a message using the broadcaster's current channel.</summary>
        /// <param name="line">The text to send.</param>
        public async Task SendMessage(string line)
        {
            await Send(line);
        }

        /// <summary>
        /// Sends a message using a temporarily overridden channel, then restores the original channel.
        /// </summary>
        /// <param name="line">The text to send.</param>
        /// <param name="messageType">Channel to use for this single message.</param>
        public async Task SendMessage(string line, MessageType messageType)
        {
            var oldType = MessageType;
            SetType(messageType);
            await Send(line);
            SetType(oldType);
        }

        /// <summary>Changes the chat channel used by this broadcaster.</summary>
        /// <param name="messageType">The new channel to use.</param>
        public void SetType(MessageType messageType)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// Sends a raw UTF-8 byte array as a chat message by calling the game's <c>ExecuteCommandInner</c> function directly,
        /// bypassing RebornBuddy's <see cref="ff14bot.Managers.ChatManager"/>.
        /// </summary>
        /// <param name="array">The complete message byte payload including channel prefix.</param>
        public static void SendChat(byte[] array)
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
            Core.Memory.CallInjectedWraper<int>(Offsets.ExecuteCommandInner,
                                                UiManagerProxy.RaptureShellModule,
                                                allocatedMemory2.Address,
                                                UiManagerProxy.UIModule);
        }
    }
}