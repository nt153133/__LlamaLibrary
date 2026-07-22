using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the main housing interface and system state.
    /// Manages client-side interaction with general housing features, purchase windows, and estate setup.
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