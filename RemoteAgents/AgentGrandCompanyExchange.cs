using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Grand Company Exchange interface.
    /// Handles purchasing items with Grand Company Seals and tracking selected categories/ranks.
    /// </summary>
    public class AgentGrandCompanyExchange : AgentInterface<AgentGrandCompanyExchange>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentGrandCompanyExchangeOffsets.Vtable;

        /// <summary>
        /// Gets the currently selected category in the Grand Company Exchange window.
        /// </summary>
        public byte Category => Core.Memory.Read<byte>(Pointer + AgentGrandCompanyExchangeOffsets.Category);

        /// <summary>
        /// Gets the currently selected rank filter in the Grand Company Exchange window.
        /// </summary>
        public byte Rank => Core.Memory.Read<byte>(Pointer + AgentGrandCompanyExchangeOffsets.Rank);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentGrandCompanyExchange"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentGrandCompanyExchange(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Purchases a specific item from the Grand Company Exchange.
        /// </summary>
        /// <param name="index">The zero-based index of the item in the list.</param>
        /// <param name="qty">The quantity of the item to purchase.</param>
        public void BuyItem(uint index, int qty)
        {
            Core.Memory.CallInjectedWraper<IntPtr>(AgentGrandCompanyExchangeOffsets.BuyItem, GrandCompanyShop.ActiveShopPtr, 0, index, qty);
        }
    }
}