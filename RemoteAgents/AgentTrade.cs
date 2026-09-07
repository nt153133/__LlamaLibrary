using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents;

/// <summary>
/// Remote agent for player-to-player trading.
/// </summary>
public class AgentTrade : AgentInterface<AgentTrade>, IAgent
{
    /// <inheritdoc/>
    public IntPtr RegisteredVtable => AgentTradeOffsets.VTable;

    /// <summary>Initializes a wrapper for the trade agent.</summary>
    /// <param name="pointer">The agent's primary interface address.</param>
    protected AgentTrade(IntPtr pointer) : base(pointer)
    {
    }

    /// <summary>
    /// Gets the secondary inventory callback interface, or zero if its handler does not match the trade callback.
    /// </summary>
    public IntPtr InventoryContextEvent
    {
        get
        {
            if (Pointer == IntPtr.Zero || AgentTradeOffsets.InventoryContextEventOffset <= 0 || BagSlotExtensionsOffsets.TradeBagSlot == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var callback = Pointer + AgentTradeOffsets.InventoryContextEventOffset;
            var vtable = Core.Memory.Read<IntPtr>(callback);
            return vtable != IntPtr.Zero && Core.Memory.Read<IntPtr>(vtable) == BagSlotExtensionsOffsets.TradeBagSlot
                ? callback
                : IntPtr.Zero;
        }
    }
}
