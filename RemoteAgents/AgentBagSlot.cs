using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Wrapper over the inventory-context agent used to access registered item-context handlers.
    /// </summary>
    public class AgentBagSlot : AgentInterface<AgentBagSlot>, IAgent
    {
        private const int ContextCallbackInfoSize = 0x20;
        private const int AetherialWheelCallbackIndex = 7;

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
        /// Gets the inventory-context callback handler registered by the aetherial wheel agent.
        /// </summary>
        public IntPtr PointerForAether => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + AgentBagSlotOffsets.Offset) + (ContextCallbackInfoSize * AetherialWheelCallbackIndex) + AgentBagSlotOffsets.FuncOffset);
    }
}
