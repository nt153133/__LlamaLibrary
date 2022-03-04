using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.ClientDataHelpers;

public static class AtkArrayDataHolder
{
    private static class Offsets
    {
        [Offset("Search 41 FF 52 ? 49 8B 4D ? 4C 8B E0 Add 3 Read8")]
        internal static int AtkModule_vf9;

        [Offset("Search 41 FF 50 ? 48 8B E8 4D 85 E4 Add 3 Read8")]
        internal static int AtkModule_vfStringArray;

    }

    private static IntPtr RaptureAtkModule;
    private static IntPtr GetNumArrayFunction;
    private static IntPtr GetStringArrayFunction;

    static AtkArrayDataHolder()
    {
        RaptureAtkModule = UiManagerProxy.RaptureAtkModule;
        var vtable = Core.Memory.Read<IntPtr>(RaptureAtkModule);
        GetNumArrayFunction = Core.Memory.Read<IntPtr>(vtable + Offsets.AtkModule_vf9);
        GetStringArrayFunction = Core.Memory.Read<IntPtr>(vtable + Offsets.AtkModule_vfStringArray);
    }

    public static IntPtr GetNumberArray(int index) => Core.Memory.CallInjected64<IntPtr>(GetNumArrayFunction, RaptureAtkModule, index);

    public static IntPtr GetStringArray(int index) => Core.Memory.CallInjected64<IntPtr>(GetStringArrayFunction, RaptureAtkModule, index);
}