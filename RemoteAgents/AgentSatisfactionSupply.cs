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

public class AgentSatisfactionSupply : AgentInterface<AgentSatisfactionSupply>, IAgent
{
    public const int DoHDeliverable = 0;
    public const int DoLDeliverable = 1;
    public const int FshDeliverable = 2;

    private CustomDelivery[] _customDeliveries;
    private uint _lastNpcId;

    protected AgentSatisfactionSupply(IntPtr pointer) : base(pointer)
    {
    }

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

    public byte DeliveriesRemaining => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.DeliveriesRemaining);
    public byte HeartLevel => Core.Memory.Read<byte>(Pointer + AgentSatisfactionSupplyOffsets.HeartLevel);
    public uint DoHItemId => CustomDeliveries[DoHDeliverable].DeliverableItemId;
    public uint DoLItemId => CustomDeliveries[DoLDeliverable].DeliverableItemId;
    public uint FshItemId => CustomDeliveries[FshDeliverable].DeliverableItemId;

    public CustomDelivery[] ReadDeliveries => Core.Memory.ReadArray<CustomDelivery>(Pointer + AgentSatisfactionSupplyOffsets.CustomDeliveryArray, 3);

    public uint NpcId => Core.Memory.Read<uint>(Pointer);

    public uint CurrentRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.CurrentRep);
    public uint MaxRep => Core.Memory.Read<ushort>(Pointer + AgentSatisfactionSupplyOffsets.MaxRep);

    public bool HasDoHTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoHItemId);
    public bool HasDoLTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.DoLItemId);
    public bool HasFshTurnin => InventoryManager.FilledSlots.Any(i => i.RawItemId == Instance.FshItemId);

    public bool HasAnyTurnin => HasDoHTurnin || HasDoLTurnin || HasFshTurnin;

    public IntPtr DeliveriesPointer => Pointer + AgentSatisfactionSupplyOffsets.CustomDeliveryArray;

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

    public IntPtr RegisteredVtable => AgentSatisfactionSupplyOffsets.VTable;

    public string PrintDeliverables()
    {
        return $"DoH: {CustomDeliveries[DoHDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[DoHDeliverable].DeliverableItemId}), " +
               $"DoL: {CustomDeliveries[DoLDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[DoLDeliverable].DeliverableItemId}), " +
               $"FSH: {CustomDeliveries[FshDeliverable].Item.CurrentLocaleName} ({CustomDeliveries[FshDeliverable].DeliverableItemId})";
    }
}