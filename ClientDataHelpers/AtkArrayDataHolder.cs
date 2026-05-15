using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.ClientDataHelpers
{
    public static class AtkArrayDataHolder
    {
        private static readonly IntPtr RaptureAtkModule;
        private static readonly IntPtr GetNumArrayFunction;
        private static readonly IntPtr GetStringArrayFunction;

        private static readonly IntPtr NumberArrayData;
        private static readonly IntPtr StringArrayData;
        private static readonly IntPtr ExtendArrayData;

        public static IntPtr AtkStage => Core.Memory.Read<IntPtr>(LlamaLibrary.Memory.Offsets.AtkStage);
        public static IntPtr GetNumberArrayDataArray()
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetNumberArrayData, AtkStage);
        }

        public static IntPtr GetStringArrayDataArray()
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetStringArrayData, AtkStage);
        }

        public static IntPtr GetExtendArrayDataArray()
        {
            return Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetStringArrayData, AtkStage);
        }

        static AtkArrayDataHolder()
        {
            RaptureAtkModule = UiManagerProxy.RaptureAtkModule;
            var vtable = Core.Memory.Read<IntPtr>(RaptureAtkModule);
            GetNumArrayFunction = Core.Memory.Read<IntPtr>(vtable + AtkArrayDataHolderOffsets.AtkModule_vf9);
            GetStringArrayFunction = Core.Memory.Read<IntPtr>(vtable + AtkArrayDataHolderOffsets.AtkModule_vfStringArray);

            NumberArrayData = GetNumberArrayDataArray();
            StringArrayData = GetStringArrayDataArray();
            ExtendArrayData = GetExtendArrayDataArray();
        }

        [Obsolete("Use NumberArray instead.")]
        public static IntPtr GetNumberArray(int index)
        {
            return NumberArray(index);
        }

        public static IntPtr NumberArray(int index) => Core.Memory.Read<IntPtr>(NumberArrayData + index * 0x8);

        public static IntPtr StringArray(int index) => Core.Memory.Read<IntPtr>(StringArrayData + index * 0x8);

        public static IntPtr ExtendArray(int index) => Core.Memory.Read<IntPtr>(ExtendArrayData + index * 0x8);

        [Obsolete("Use StringArray instead.")]
        public static IntPtr GetStringArray(int index)
        {
            return StringArray(index);
        }
    }
}