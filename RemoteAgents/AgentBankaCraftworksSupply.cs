using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Banka Craftworks item supply interface.
/// Manages the submission of crafted or gathered items for Banka Craftworks.
/// </summary>
public class AgentBankaCraftworksSupply : AgentInterface<AgentBankaCraftworksSupply>, IAgent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentBankaCraftworksSupply"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
    protected AgentBankaCraftworksSupply(IntPtr pointer) : base(pointer)
    {
    }

    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentBankaCraftworksSupplyOffsets.Vtable;

    /// <summary>
    /// Gets the memory address of the internal instance pointer.
    /// </summary>
    public IntPtr InstancePointer
    {
        get
        {
            var temp1 = Core.Memory.Read<IntPtr>(Pointer);
            return temp1;
        }
    }

    /// <summary>
    /// Gets the memory address of the internal function pointer used for item hand-ins,
    /// resolved by reading an offset from the main instance pointer.
    /// </summary>
    public IntPtr InstancePointerWithOffset
    {
        get
        {
            var temp1 = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer) + AgentBankaCraftworksSupplyOffsets.PointerOffset);
            return temp1;
        }
    }

    /// <summary>
    /// Executes the hand-in operation for a specified item in a bag slot.
    /// </summary>
    /// <param name="slot">The <see cref="BagSlot"/> containing the item to hand in.</param>
    public void HandIn(BagSlot slot)
    {
        //var instance = Core.Memory.Read<IntPtr>(Pointer) + Offsets.PointerOffset;
        Core.Memory.CallInjectedWraper<uint>(InstancePointerWithOffset,
                                             Pointer,
                                             slot.Slot,
                                             (int)slot.BagId);
    }
}
