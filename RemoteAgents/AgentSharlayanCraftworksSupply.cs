using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

public class AgentSharlayanCraftworksSupply : AgentInterface<AgentSharlayanCraftworksSupply>, IAgent
{
    public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentSharlayanCraftworksSupply;

    protected AgentSharlayanCraftworksSupply(IntPtr pointer) : base(pointer)
    {
    }

    /*public void HandIn(BagSlot slot)
    {
        Core.Memory.CallInjectedWraper<uint>(
                                         Offsets.HandIn,
                                         Pointer + Offsets.PointerOffset,
                                         slot.Slot,
                                         (int)slot.BagId);
    }*/
    public void HandIn(BagSlot slot)
    {
#if RB_TC
        var instance = Pointer + 0x30; //+ Offsets.PointerOffset;
#else
        var instance = Pointer + 0x28; //+ Offsets.PointerOffset;
#endif
        Core.Memory.CallInjectedWraper<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(instance)),
                                             instance,
                                             slot.Slot,
                                             (int)slot.BagId);
    }
}