using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "InventoryBuddy" window (Chocobo Saddlebag).
    /// Handles the data and logic for managing items stored in the player's chocobo saddlebags.
    /// </summary>
    public class AgentInventoryBuddy : AgentInterface<AgentInventoryBuddy>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentInventoryBuddyOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentInventoryBuddy"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentInventoryBuddy(IntPtr pointer) : base(pointer)
        {
        }
    }
}