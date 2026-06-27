using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Island Sanctuary (MJI) pouch interface.
    /// Handles data and logic for the specialized inventory used within the Island Sanctuary.
    /// </summary>
    public class AgentMJIPouch : AgentInterface<AgentMJIPouch>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMJIPouchOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMJIPouch"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMJIPouch(IntPtr pointer) : base(pointer)
        {
        }
    }
}
