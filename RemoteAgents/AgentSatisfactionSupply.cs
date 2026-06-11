using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for the Custom Deliveries (Satisfaction Supply) interface.
/// Handles information about available deliverables, remaining allowances, and NPC relationship progress.
/// </summary>
public class AgentSatisfactionSupply : AgentInterface<AgentSatisfactionSupply>, IAgent
{
    /// <summary>The index in the delivery array for Disciple of the Hand (crafting) items.</summary>
    public const int DoHDeliverable = 0;

    /// <summary>The index in the delivery array for Disciple of the Land (gathering) items.</summary>
    public const int DoLDeliverable = 1;

    /// <summary>The index in the delivery array for Fisher items.</summary>
    public const int FshDeliverable = 2;

    private CustomDelivery[] _customDeliveries;
    private uint _lastNpcId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentSatisfactionSupply"/> class.
    /// </summary>
    /// <param name="pointer">The memory address of the agent.</param>
    protected AgentSatisfactionSupply(IntPtr pointer) : base(pointer)
    {
    }

    /// <summary>
    /// Gets the list of available custom deliveries for the currently selected NPC.
    /// Results are cached and updated only when the NPC ID changes.
    /// </summary>
    public CustomDelivery[] CustomDeliveries
    {
        get
        {
            if (_customDeliveries == null || NpcId != _lastNpcId)
            {
                _customDeliveries = ReadDeliveries;
                _lastNpcId = NpcId;
            }

            return _customDeliveries;
        }
    }

    /// <summary>Gets the number of custom delivery allowances remaining for the week.</summary>
    public byte DeliveriesRemaining => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.DeliveriesRemaining);

    /// <summary>Gets the current satisfaction heart level (1-5) with the NPC.</summary>
    public byte HeartLevel => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.HeartLevel);

    /// <summary>Gets the item ID required for the current Disciple of the Hand delivery.</summary>
    public uint DoHItemId => CustomDeliveries[DoHDeliverable].DeliverableItemId;

    /// <summary>Gets the item ID required for the current Disciple of the Land delivery.</summary>
    public uint DoLItemId => CustomDeliveries[DoLDeliverable].DeliverableItemId;

    /// <summary>Gets the item ID required for the current Fisher delivery.</summary>
    public uint FshItemId => CustomDeliveries[FshDeliverable].DeliverableItemId;

    /// <summary>Reads the raw array of 3 <see cref="CustomDelivery"/> structures from game memory.</summary>
    public CustomDelivery[] ReadDeliveries => Core.Memory.ReadArray<CustomDelivery>(Pointer + AgentSatisfactionSupplyOffsets.CustomDeliveryArray, 3);

    /// <summary>Gets the ENpcResident ID of the currently active custom delivery NPC.</summary>
    public uint NpcId => Core.Memory.Read<uint>(Pointer);

    /// <summary>Gets the current satisfaction points toward the next heart level.</summary>
    public uint CurrentRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.CurrentRep);

    /// <summary>Gets the total satisfaction points required for the next heart level.</summary>
    public uint MaxRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.MaxRep);

    /// <summary>Gets a value indicating whether the player's inventory contains the required item for the crafting delivery.</summary>
    public bool HasDoHTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoHItemId);

    /// <summary>Gets a value indicating whether the player's inventory contains the required item for the gathering delivery.</summary>
    public bool HasDoLTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoLItemId);

    /// <summary>Gets a value indicating whether the player's inventory contains the required item for the fishing delivery.</summary>
    public bool HasFshTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.FshItemId);

    /// <summary>Gets a value indicating whether the player's inventory contains any items for the currently selected NPC's deliveries.</summary>
    public bool HasAnyTurnin => HasDoHTurnin || HasDoLTurnin || HasFshTurnin;

    /// <summary>Gets the memory pointer to the start of the custom delivery data array.</summary>
    public IntPtr DeliveriesPointer => Pointer + AgentSatisfactionSupplyOffsets.CustomDeliveryArray;

    /// <summary>
    /// Forces the custom delivery window to load for the specified NPC by calling the game's internal open function.
    /// This updates the agent's memory state with the NPC's specific requirements.
    /// </summary>
    /// <param name="npc">The ENpcResident ID of the NPC to load.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadWindow(uint npc)
    {
        if (SatisfactionSupply.Instance.IsOpen)
        {
            SatisfactionSupply.Instance.Close();
            await Coroutine.Wait(5000, () => !SatisfactionSupply.Instance.IsOpen);
        }

        Core.Memory.CallInjectedWraper<IntPtr>(AgentSatisfactionSupplyOffsets.OpenWindow,
                                               Pointer,
                                               0U,
                                               npc,
                                               1U);

        await Coroutine.Wait(5000, () => SatisfactionSupply.Instance.IsOpen);
        _customDeliveries = ReadDeliveries;
        _lastNpcId = NpcId;

        if (SatisfactionSupply.Instance.IsOpen)
        {
            SatisfactionSupply.Instance.Close();
            await Coroutine.Wait(5000, () => !SatisfactionSupply.Instance.IsOpen);
        }
    }

    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentSatisfactionSupplyOffsets.VTable;

    /// <summary>
    /// Generates a formatted string listing the current deliverable items and their IDs for all three categories.
    /// </summary>
    /// <returns>A string in the format: "DoH: ItemName (ID), DoL: ItemName (ID), FSH: ItemName (ID)".</returns>
    public string PrintDeliverables()
    {
        return $"DoH: {CustomDeliveries[DoHDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[DoHDeliverable].DeliverableItemId}), " +
               $"DoL: {CustomDeliveries[DoLDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[DoLDeliverable].DeliverableItemId}), " +
               $"FSH: {CustomDeliveries[FshDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[FshDeliverable].DeliverableItemId})";
    }
}