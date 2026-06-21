using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Journal Detail window.
    /// Manages the display of detailed information for selected quests or duties in the journal.
    /// </summary>
    public class AgentJournalDetail : AgentInterface<AgentJournalDetail>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentJournalDetailOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentJournalDetail"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentJournalDetail(IntPtr pointer) : base(pointer)
        {
        }
    }
}