using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Island Sanctuary (MJI) recipe notebook interface.
    /// Manages the data for crafting recipes available within the Island Sanctuary.
    /// </summary>
    public class AgentMJIRecipeNoteBook : AgentInterface<AgentMJIRecipeNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMJIRecipeNoteBookOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMJIRecipeNoteBook"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMJIRecipeNoteBook(IntPtr pointer) : base(pointer)
        {
        }
    }
}
