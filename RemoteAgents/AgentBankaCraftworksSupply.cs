using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;


public class AgentBankaCraftworksSupply : AgentInterface<AgentBankaCraftworksSupply>, IAgent
{


    protected AgentBankaCraftworksSupply(IntPtr pointer) : base(pointer)
    {
    }

    public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentBankaCraftworksSupply;

    public IntPtr InstancePointer
    {
        get
        {
            var temp1 = Core.Memory.Read<IntPtr>(Pointer ) ;
            return temp1;
        }
    }

    public IntPtr InstancePointerWithOffset
    {
        get
        {
            var temp1 = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer) + AgentBankaCraftworksSupplyOffsets.PointerOffset);
            return temp1;
        }
    }

    public void HandIn(BagSlot slot)
    {
        //var instance = Core.Memory.Read<IntPtr>(Pointer) + Offsets.PointerOffset;
        Core.Memory.CallInjectedWraper<uint>(InstancePointerWithOffset,
                                             Pointer,
                                             slot.Slot,
                                             (int)slot.BagId);
    }
}
