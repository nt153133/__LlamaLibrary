using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers.MessageHandling
{
    /// <summary>
    /// Central dispatcher for in-game chat messages received via <see cref="GamelogManager"/>.
    /// Consumers register an <see cref="IChatReceiver"/> to be called whenever a message of a
    /// matching <see cref="ff14bot.Enums.MessageType"/> is received.
    /// </summary>
    /// <remarks>
    /// The underlying <see cref="GamelogManager.MessageRecevied"/> event is only subscribed while
    /// at least one receiver is registered, preventing unnecessary overhead.
    /// </remarks>
    public static class ChatHandler
    {
        private static readonly List<IChatReceiver> Receivers = new List<IChatReceiver>();

        /// <summary>
        /// Registers <paramref name="receiver"/> to receive future chat messages.
        /// If this is the first registered receiver the <see cref="GamelogManager"/> event hook
        /// is automatically attached.
        /// Duplicate registrations are silently ignored.
        /// </summary>
        /// <param name="receiver">The <see cref="IChatReceiver"/> to register.</param>
        public static void RegisterReceiver(IChatReceiver receiver)
        {
            if (Receivers.Count == 0)
            {
                GamelogManager.MessageRecevied -= GamelogManager_MessageReceived;
                GamelogManager.MessageRecevied += GamelogManager_MessageReceived;
            }

            if (!Receivers.Contains(receiver))
            {
                Receivers.Add(receiver);
            }
        }

        /// <summary>
        /// Removes <paramref name="receiver"/> from the dispatch list.
        /// If no receivers remain after removal, the <see cref="GamelogManager"/> event hook
        /// is automatically detached.
        /// </summary>
        /// <param name="receiver">The <see cref="IChatReceiver"/> to unregister.</param>
        public static void UnregisterReceiver(IChatReceiver receiver)
        {
            if (Receivers.Contains(receiver))
            {
                Receivers.Remove(receiver);
            }

            if (Receivers.Count == 0)
            {
                GamelogManager.MessageRecevied -= GamelogManager_MessageReceived;
            }
        }

        private static void GamelogManager_MessageReceived(object? sender, ChatEventArgs e)
        {
            foreach (var receiver in Receivers.Where(receiver => receiver.TypesToReceive.Contains(e.ChatLogEntry.MessageType)))
            {
                receiver.ProcessMessage(e.ChatLogEntry.Contents, e.ChatLogEntry.TimeStamp);
            }
        }

        private static void ClearReceivers()
        {
            Receivers.Clear();
        }

        /// <summary>
        /// Removes all registered receivers and detaches the <see cref="GamelogManager"/> event hook.
        /// Call this during cleanup to prevent memory leaks or stale callbacks.
        /// </summary>
        public static void UnregisterAllReceivers()
        {
            ClearReceivers();
            GamelogManager.MessageRecevied -= GamelogManager_MessageReceived;
        }
    }
}