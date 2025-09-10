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

        static AtkArrayDataHolder()
        {
            RaptureAtkModule = UiManagerProxy.RaptureAtkModule;
            var vtable = Core.Memory.Read<IntPtr>(RaptureAtkModule);
            GetNumArrayFunction = Core.Memory.Read<IntPtr>(vtable + AtkArrayDataHolderOffsets.AtkModule_vf9);
            GetStringArrayFunction = Core.Memory.Read<IntPtr>(vtable + AtkArrayDataHolderOffsets.AtkModule_vfStringArray);
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