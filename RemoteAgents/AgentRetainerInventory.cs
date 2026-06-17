using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using System;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the retainer inventory interface.
    /// Manages the data and logic for viewing and interacting with a retainer's current inventory.
    /// </summary>
    public class AgentRetainerInventory : AgentInterface<AgentRetainerInventory>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentRetainerInventoryOffsets.VTable;
        // ReSharper disable once PartialTypeWithSinglePart

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRetainerInventory"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentRetainerInventory(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the memory pointer to the retainer's shop interface data.
        /// </summary>
        public IntPtr RetainerShopPointer => Core.Memory.Read<IntPtr>(Pointer + AgentRetainerInventoryOffsets.ShopOffset);
    }
}