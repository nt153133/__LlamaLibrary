using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "JournalDetail" window.
    /// Displays detailed information for a selected quest or duty.
    /// </summary>
    public class JournalDetail : RemoteWindow<JournalDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalDetail"/> class.
        /// </summary>
        public JournalDetail() : base("JournalDetail")
        {
        }

        /// <summary>
        /// Sets the quest to display details for using its global ID.
        /// </summary>
        /// <param name="globalId">The global identifier of the quest.</param>
        public void SetQuest(ulong globalId)
        {
            SendAction(3, 3, 0xD, 3, globalId, 3, 2);
        }

        /// <summary>
        /// Initiates the guildleve specified by its global ID.
        /// </summary>
        /// <param name="globalId">The global identifier of the guildleve.</param>
        public void InitiateLeve(ulong globalId)
        {
            SendAction(2, 3, 4, 4, globalId);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            SendAction(1, 3uL, 0xFFFFFFFFuL);
        }

        /// <summary>
        /// Abandons the active quest specified by its global ID.
        /// </summary>
        /// <param name="globalId">The global identifier of the quest to abandon.</param>
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