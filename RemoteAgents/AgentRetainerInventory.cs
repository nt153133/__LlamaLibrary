using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using System;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentRetainerInventory : AgentInterface<AgentRetainerInventory>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentRetainerInventory;
        // ReSharper disable once PartialTypeWithSinglePart


        protected AgentRetainerInventory(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RetainerShopPointer => Core.Memory.Read<IntPtr>(Pointer + AgentRetainerInventoryOffsets.ShopOffset);
    }
}