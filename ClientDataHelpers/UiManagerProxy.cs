using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ff14bot;
using ff14bot.Managers;

namespace LlamaLibrary.ClientDataHelpers
{
    public static class UiManagerProxy
    {
        private static readonly PropertyInfo[] Properties;

        private static readonly Dictionary<string, int> VFunctionIds = new Dictionary<string, int>()
        {
            { "AcquaintanceModule", 15 },
            { "GetFieldMarkerModule", 49 },
            { "GetRecommendEquipModule", 32 },
        };

        static UiManagerProxy()
        {
            Properties = typeof(DataManager).Assembly.GetType("ff14bot.Managers.UiManager")
                .GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

        public static IntPtr VFunctionCall(int index) => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(index), UIModule);
        public static IntPtr VFunctionAddress(int index) => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(UIModule) + 0x8 * index);

        public static IntPtr AcquaintanceModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["AcquaintanceModule"]), UIModule);

        public static IntPtr FieldMarkerModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetFieldMarkerModule"]), UIModule);

        public static IntPtr RecommendEquipModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetRecommendEquipModule"]), UIModule);

        public static IntPtr UIModule => (IntPtr)Properties.First(i => i.Name.Equals("UIModule")).GetValue(null);

        public static IntPtr RaptureAtkModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureAtkModule")).GetValue(null);

        public static IntPtr RaptureShellModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureShellModule")).GetValue(null);

        public static IntPtr RaptureTeleportHistory => (IntPtr)Properties.First(i => i.Name.Equals("RaptureTeleportHistory")).GetValue(null);

        public static IntPtr RaptureLogModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureLogModule")).GetValue(null);

        public static IntPtr RaptureGearsetModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureGearsetModule")).GetValue(null);

        public static IntPtr RaptureHotbarModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureHotbarModule")).GetValue(null);

        public static IntPtr PronounModule => (IntPtr)Properties.First(i => i.Name.Equals("PronounModule")).GetValue(null);

        public static IntPtr UIInputModule => (IntPtr)Properties.First(i => i.Name.Equals("UIInputModule")).GetValue(null);

        public static IntPtr UIInputModule_Topic => (IntPtr)Properties.First(i => i.Name.Equals("UIInputModule_Topic")).GetValue(null);
    }
}