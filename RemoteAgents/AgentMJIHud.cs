using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Island Sanctuary (MJI) HUD interface.
    /// Provides access to core progression data like current experience points.
    /// </summary>
    public class AgentMJIHud : AgentInterface<AgentMJIHud>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMJIHudOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMJIHud"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMJIHud(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the memory pointer to the Island Sanctuary information block.
        /// </summary>
        public IntPtr InfoPtr => Core.Memory.Read<IntPtr>(Pointer + AgentMJIHudOffsets.InfoPtr);

        /// <summary>
        /// Gets the player's current experience points within the Island Sanctuary.
        /// </summary>
        public uint CurrentExp => Core.Memory.Read<uint>(InfoPtr + AgentMJIHudOffsets.CurrentExp);
    }
}
