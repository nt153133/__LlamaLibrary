using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the item detail tooltip.
    /// Manages information about the item currently being hovered over in the UI.
    /// </summary>
    public class AgentItemDetail : AgentInterface<AgentItemDetail>, IAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentItemDetail"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentItemDetail(IntPtr pointer) : base(pointer)
        {
        }

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentItemDetailOffsets.Vtable;

        /// <summary>
        /// Gets the ID of the item currently being hovered over in the game UI.
        /// </summary>
        public uint HoverOverItemID => Core.Memory.Read<uint>(Pointer + AgentItemDetailOffsets.ItemID);
    }
}