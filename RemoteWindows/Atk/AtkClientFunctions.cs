using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows.Atk;

internal static class AtkClientFunctions
{
    public static void SendActionNew(this RemoteWindow window, bool updateState = true, params AtkValue[] parms)
    {
        SendActionRemote(window, updateState, parms);
    }

    public static void SendActionRemote(RemoteWindow window, bool updateState = true, params AtkValue[] parms)
    {
        var param = new ulong[parms.Length * 2];
        for (var i = 0; i < parms.Length; i++)
        {
            ulong[] temp = parms[i];
            param[i * 2] = temp[0];
            param[(i * 2) + 1] = temp[1];
        }

        SendActionRaw(window.WindowByName?.Pointer, updateState, param);

        foreach (var atkValue in parms)
        {
            atkValue.Dispose();
        }
    }

    public static void SendActionPtr(IntPtr window, bool updateState = true, params AtkValue[] parms)
    {
        var param = new ulong[parms.Length * 2];
        for (var i = 0; i < parms.Length; i++)
        {
            ulong[] temp = parms[i];
            param[i * 2] = temp[0];
            param[(i * 2) + 1] = temp[1];
        }

        SendActionRaw(window, updateState, param);

        foreach (var atkValue in parms)
        {
            atkValue.Dispose();
        }
    }

    internal static void SendActionPtr(IntPtr window, params AtkValue[] parms)
    {
        SendActionPtr(window, true, parms);
    }

    private static void SendActionRaw(IntPtr? windowPtr, bool updateState = true, params ulong[] param)
    {
        if (windowPtr == null || windowPtr == IntPtr.Zero)
        {
            throw new Exception("WindowPtr is null");
        }

        if (param.Length % 2 != 0)
        {
            throw new Exception("Param length is not even");
        }

        using var allocated = Core.Memory.CreateAllocatedMemory(param.Length * 16);
        for (var i = 0; i < param.Length; i++)
        {
            allocated.Write(i * 8, param[i]);
        }

        lock (Core.Memory.GetLock())
        {
            Core.Memory.CallInjected64<IntPtr>(Offsets.SendAction, windowPtr, param.Length / 2, allocated.Address, (byte)(updateState ? 1 : 0));
        }
    }

    internal static void ClickDialogueOkay()
    {
        var window = RaptureAtkUnitManager.GetWindowByName("Dialogue");
        if (window == null)
        {
            return;
        }

        Core.Memory.CallInjected64<IntPtr>(Offsets.DialogueOkay, window.Pointer, 0x19);
    }
}