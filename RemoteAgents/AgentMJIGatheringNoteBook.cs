using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Island Sanctuary (MJI) gathering notebook interface.
    /// Manages the data for the log of materials that can be gathered within the Island Sanctuary.
    /// </summary>
    public class AgentMJIGatheringNoteBook : AgentInterface<AgentMJIGatheringNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMJIGatheringNoteBookOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMJIGatheringNoteBook"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMJIGatheringNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}
