using System;
using ff14bot;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteAgents
{
    // Agent ID 200
    public class AgentFreeCompanyCreditShop : AgentInterface<AgentFreeCompanyCreditShop>, IAgent
    {
        public uint FreeCompanyCredits => Core.Memory.Read<uint>(Pointer + LlamaLibrary.Memory.PlatypusOffsets.FreeCompanyCredits);

        protected AgentFreeCompanyCreditShop(IntPtr pointer) : base(pointer)
        {}

        public IntPtr RegisteredVtable => LlamaLibrary.Memory.PlatypusOffsets.AgentFreeCompanyCreditShopVTable;
    }
}