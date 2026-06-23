using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the item hand-in interface (e.g., for Grand Company deliveries or quest turn-ins).
    /// Facilitates the selection and submission of items from the player's inventory.
    /// </summary>
    public class AgentHandIn : AgentInterface<AgentHandIn>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentHandInOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHandIn"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentHandIn(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Executes the hand-in action for a specific item slot.
        /// </summary>
        /// <param name="slot">The <see cref="BagSlot"/> containing the item to be handed in.</param>
        public void HandIn(BagSlot slot)
        {
            Core.Memory.CallInjectedWraper<uint>(
                                                 Offsets.HandInFunc,
                                                 Pointer + AgentHandInOffsets.HandinParmOffset,
                                                 slot.Slot,
                                                 (int)slot.BagId);
        }
    }
}