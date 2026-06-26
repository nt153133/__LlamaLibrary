using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Banka Craftworks supply interface.
/// Handles the exchange of crafted items for rewards within the Banka Craftworks system.
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
    /// Gets the primary memory pointer for the agent instance.
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
    /// Gets the memory pointer for the agent instance adjusted by its internal offset.
    /// Used as the target for hand-in function calls.
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
    /// <remarks>
    /// This method calls an internal game function using the offset-adjusted instance pointer and the target slot's index and bag ID.
    /// </remarks>
    public void HandIn(BagSlot slot)
    {
        //var instance = Core.Memory.Read<IntPtr>(Pointer) + Offsets.PointerOffset;
        Core.Memory.CallInjectedWraper<uint>(InstancePointerWithOffset,
                                             Pointer,
                                             slot.Slot,
                                             (int)slot.BagId);
    }
}
