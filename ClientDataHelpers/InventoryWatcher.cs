using System;
using ff14bot;
using LlamaLibrary.Hooks;

namespace LlamaLibrary.ClientDataHelpers;

public static class InventoryWatcher
{
    public static event EventHandler<EventArgs>? InventoryUpdated;
    public static ulong LastTick { get; private set; }

    public static void OnInventoryUpdated()
    {
        InventoryUpdated?.Invoke(null, EventArgs.Empty);
    }

    public static void Pulse()
    {
        var hook = PatchManager.GetHook<InventoryUpdatePatch>();
        if (hook?.Enable != true || InventoryUpdatePatch.TickPtr == IntPtr.Zero)
        {
            return;
        }

        var tick = Core.Memory.Read<ulong>(InventoryUpdatePatch.TickPtr);
        if (tick == LastTick)
        {
            return;
        }

        LastTick = tick;
        OnInventoryUpdated();
    }
}
