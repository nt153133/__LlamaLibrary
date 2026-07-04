using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Sharlayan Craftworks item submission interface.
/// Facilitates the hand-in of crafted items for the Sharlayan Craftworks system.
/// </summary>
public class AgentSharlayanCraftworksSupply : AgentInterface<AgentSharlayanCraftworksSupply>, IAgent
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentSharlayanCraftworksSupplyOffsets.Vtable;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentSharlayanCraftworksSupply"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
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

    /// <summary>
    /// Hands in the specified <see cref="BagSlot"/> to the Sharlayan Craftworks interface.
    /// </summary>
    /// <param name="slot">The inventory slot containing the item to be handed in.</param>
    /// <remarks>
    /// The memory offset for the instance pointer differs between the standard and Chinese (TC) clients.
    /// </remarks>
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