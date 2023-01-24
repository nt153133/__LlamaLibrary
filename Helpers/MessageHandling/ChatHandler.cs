using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers.MessageHandling
{
    public static class ChatHandler
    {
        private static readonly List<IChatReceiver> Receivers = new List<IChatReceiver>();

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

        private static void GamelogManager_MessageReceived(object sender, ChatEventArgs e)
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

        public static void UnregisterAllReceivers()
        {
            ClearReceivers();
            GamelogManager.MessageRecevied -= GamelogManager_MessageReceived;
        }
    }
}