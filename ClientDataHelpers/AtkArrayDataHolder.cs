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

        /*
        [Offset("Search 48 8B 41 ? 48 63 D2 44 39 04 90 Add 3 Read8")]
        internal static int NumberArrayData_IntArray;

        [Offset("Search BA ? ? ? ? 49 8B CC E8 ? ? ? ? 4C 8B 7C 24 ? 48 8B 74 24 ? Add 1 Read32")]
        internal static int NumberArrayData_Count;

        [Offset("Search BF ? ? ? ? 41 BE ? ? ? ? 90 Add 7 Read32")]
        internal static int NumberArrayData_Start;

        [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 10 41 FF 52 ? 49 8B 4D ? 4C 8B E0 Add 1 Read32")]
        internal static int vf9Param;

        [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B E8 4D 85 E4 Add 1 Read32")]
        internal static int StringArrayParam;

        [Offset("Search 48 8B 43 ? 48 63 CA 45 84 C9 Add 3 Read8")]
        internal static int StringArrayData_StrArray;

        [Offset("Search BF ? ? ? ? 41 BE ? ? ? ? 90 Add 1 Read32")]
        internal static int StringArrayData_Start;
        */
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