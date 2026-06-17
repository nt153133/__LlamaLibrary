using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for interacting with individual bag slots and their associated context menus/actions.
    /// Used for specialized item actions like aetherial reduction.
    /// </summary>
    public class AgentBagSlot : AgentInterface<AgentBagSlot>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentBagSlotOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentBagSlot"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentBagSlot(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the memory pointer used for triggering aetherial reduction on the current slot.
        /// </summary>
        public IntPtr PointerForAether => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + AgentBagSlotOffsets.Offset) + (0x20 * 7) + AgentBagSlotOffsets.FuncOffset);
    }
}