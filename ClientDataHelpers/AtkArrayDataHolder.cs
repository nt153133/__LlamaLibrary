using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.ClientDataHelpers
{
    public static class AtkArrayDataHolder
    {
        private static class Offsets
        {
            [Offset("Search 41 FF 52 ? 49 8B 4D ? 4C 8B E0 Add 3 Read8")]
            [OffsetDawntrail("Search 41 FF 50 48 ? 8B 4F 08 48 8B F0 48 8B 11 FF 52 40 BA ? ? ? ? Add 4 Read8")]
            internal static int AtkModule_vf9;

            [Offset("Search 41 FF 50 ? 48 8B E8 4D 85 E4 Add 3 Read8")]
            [OffsetDawntrail("Search 41 FF 50 ? 4C 8B E0 48 85 F6 0F 84 ? ? ? ? 48 85 C0 Add 3 Read8")]
            internal static int AtkModule_vfStringArray;
        }

        private static readonly IntPtr RaptureAtkModule;
        private static readonly IntPtr GetNumArrayFunction;
        private static readonly IntPtr GetStringArrayFunction;

        static AtkArrayDataHolder()
        {
            RaptureAtkModule = UiManagerProxy.RaptureAtkModule;
            var vtable = Core.Memory.Read<IntPtr>(RaptureAtkModule);
            GetNumArrayFunction = Core.Memory.Read<IntPtr>(vtable + Offsets.AtkModule_vf9);
            GetStringArrayFunction = Core.Memory.Read<IntPtr>(vtable + Offsets.AtkModule_vfStringArray);
        }

        public static IntPtr GetNumberArray(int index)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(GetNumArrayFunction, RaptureAtkModule, index);
        }

        public static IntPtr GetStringArray(int index)
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(GetStringArrayFunction, RaptureAtkModule, index);
        }
    }
}