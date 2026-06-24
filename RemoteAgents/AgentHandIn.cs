using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the item hand-in interface.
    /// Handles the process of selecting and submitting items from the player's inventory to an NPC or interface.
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
        /// Hands in the specified <see cref="BagSlot"/> to the interface.
        /// </summary>
        /// <param name="slot">The inventory slot containing the item to be handed in.</param>
        /// <remarks>
        /// This method writes the slot's index and bag ID to a memory-mapped parameter block
        /// before calling the internal game function responsible for the hand-in action.
        /// </remarks>
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