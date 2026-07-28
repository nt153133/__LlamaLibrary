using System;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the MGP exchange interface.
    /// </summary>
    public class AgentShopExchangeCoin : AgentInterface<AgentShopExchangeCoin>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentShopExchangeCoinOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentShopExchangeCoin"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentShopExchangeCoin(IntPtr pointer) : base(pointer)
        {
        }
    }
}