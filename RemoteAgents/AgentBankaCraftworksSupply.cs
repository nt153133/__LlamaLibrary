using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;


/// <summary>
/// Remote agent for the Banka Craftworks item submission interface.
/// Facilitates the hand-in of crafted items for the Banka Craftworks system.
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
    /// Gets the memory pointer to the primary instance of the craftworks supply data.
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
    /// Gets the memory pointer to the craftworks supply data, adjusted by the system-defined offset.
    /// Used as the target for the hand-in function call.
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
    /// Hands in the specified <see cref="BagSlot"/> to the Banka Craftworks interface.
    /// </summary>
    /// <param name="slot">The inventory slot containing the item to be handed in.</param>
    public void HandIn(BagSlot slot)
    {
        //var instance = Core.Memory.Read<IntPtr>(Pointer) + Offsets.PointerOffset;
        Core.Memory.CallInjectedWraper<uint>(InstancePointerWithOffset,
                                             Pointer,
                                             slot.Slot,
                                             (int)slot.BagId);
    }
}
