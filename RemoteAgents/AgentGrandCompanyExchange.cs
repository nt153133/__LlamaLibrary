using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentGrandCompanyExchange : AgentInterface<AgentGrandCompanyExchange>, IAgent
    {
        

        public IntPtr RegisteredVtable => AgentGrandCompanyExchangeOffsets.Vtable;

        public byte Category => Core.Memory.Read<byte>(Pointer + AgentGrandCompanyExchangeOffsets.Category);
        public byte Rank => Core.Memory.Read<byte>(Pointer + AgentGrandCompanyExchangeOffsets.Rank);

        protected AgentGrandCompanyExchange(IntPtr pointer) : base(pointer)
        {
        }

        public void BuyItem(uint index, int qty)
        {
            Core.Memory.CallInjectedWraper<IntPtr>(AgentGrandCompanyExchangeOffsets.BuyItem, GrandCompanyShop.ActiveShopPtr, 0, index, qty);
        }
    }
}