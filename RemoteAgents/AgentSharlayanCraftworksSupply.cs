using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

public class AgentSharlayanCraftworksSupply : AgentInterface<AgentSharlayanCraftworksSupply>, IAgent
{


    public IntPtr RegisteredVtable => AgentSharlayanCraftworksSupplyOffsets.Vtable;

    protected AgentSharlayanCraftworksSupply(IntPtr pointer) : base(pointer)
    {
    }

    /*public void HandIn(BagSlot slot)
    {
        lock (Core.Memory.Executor.AssemblyLock)
        {
            Core.Memory.CallInjectedWraper<uint>(
                                             Offsets.HandIn,
                                             Pointer + Offsets.PointerOffset,
                                             slot.Slot,
                                             (int)slot.BagId);
        }
    }*/
    public void HandIn(BagSlot slot)
    {
        var instance = Pointer + 0x30; //+ Offsets.PointerOffset;
        lock (Core.Memory.Executor.AssemblyLock)
        {
            Core.Memory.CallInjectedWraper<uint>(Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(instance)),
                                             instance,
                                             slot.Slot,
                                             (int)slot.BagId);
        }
    }
}