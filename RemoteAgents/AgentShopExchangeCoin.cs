using System;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Provides direct access to the MGP exchange agent through its canonical agent-module slot.
    /// </summary>
    public sealed class AgentShopExchangeCoin : AgentInterface
    {
        /// <summary>
        /// The stable AgentModule slot assigned to ShopExchangeCoin by the game client.
        /// Using the slot avoids a fragile vtable signature whose fully operand-masked constructor
        /// and destructor bodies are shared by many unrelated agents.
        /// </summary>
        public const int AgentId = 187;

        private static AgentShopExchangeCoin? _instance;

        /// <summary>Gets the MGP exchange agent backed by AgentModule slot 187.</summary>
        public static AgentShopExchangeCoin Instance =>
            _instance ??= new AgentShopExchangeCoin(AgentModule.AgentPointers[AgentId]);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentShopExchangeCoin"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        private AgentShopExchangeCoin(IntPtr pointer) : base(pointer)
        {
        }
    }
}
