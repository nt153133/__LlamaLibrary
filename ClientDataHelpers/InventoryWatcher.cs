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
        if ((!PatchManager.GetHook<InventoryUpdatePatch>()?.Enable ?? false) || InventoryUpdatePatch.TickPtr == IntPtr.Zero)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic($"InventoryWatcher: Patch not enabled {PatchManager.GetHook<InventoryUpdatePatch>()?.Enable} or TickPtr {InventoryUpdatePatch.TickPtr.ToString("X")} is null.");
            return;
        }

        var tick = Core.Memory.Read<uint>(InventoryUpdatePatch.TickPtr);
        if (tick <= LastTick)
        {
            return;
        }

        LastTick = tick;
        OnInventoryUpdated();
    }
}