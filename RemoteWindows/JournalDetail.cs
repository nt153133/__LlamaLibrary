using System.Linq;
using ff14bot.Managers;
using LlamaLibrary.RemoteWindows.Atk;

namespace LlamaLibrary.RemoteWindows
{
    public class JournalDetail : RemoteWindow<JournalDetail>
    {
        public JournalDetail() : base("JournalDetail")
        {
        }

        public void SetQuest(ulong globalId)
        {
            SendAction(3, 3, 0xD, 3, globalId, 3, 2);
        }

        public void InitiateLeve(ulong globalId)
        {
            SendAction(2, 3, 4, 4, globalId);
        }

        /// <summary>Clicks Accept for the quest currently displayed in JournalDetail.</summary>
        /// <param name="questId">The quest ID supplied by the JournalDetail callback.</param>
        public void AcceptQuest(uint questId)
        {
            SendAction(true, (ValueType.Int, 0x3), (ValueType.UInt, questId));
        }

        public override void Close()
        {
            SendAction(1, 3uL, 0xFFFFFFFFuL);
        }

        public void AbandonQuest(int globalId)
        {
            var rawId = QuestLogManager.ActiveQuests.FirstOrDefault(i => i.GlobalId == globalId)?.RawId ?? 0;
            if (rawId == 0)
            {
                return;
            }

            SendAction(2, 3, 0x7, 5, rawId);
        }
    }
}
