using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing management system.
    /// Interfaces with the game's internal housing menu and state.
    /// </summary>
    public class AgentHousing : AgentInterface<AgentHousing>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentHousingOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHousing"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentHousing(IntPtr pointer) : base(pointer)
        {
        }
    }
}
