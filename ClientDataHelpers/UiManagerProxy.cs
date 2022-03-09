using System;
using System.Linq;
using System.Reflection;
using ff14bot.Managers;

namespace LlamaLibrary.ClientDataHelpers
{

    public static class UiManagerProxy
    {
        private static PropertyInfo[] properties;

        static UiManagerProxy()
        {
            properties = typeof(DataManager).Assembly.GetType("ff14bot.Managers.UiManager")
                .GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

        public static IntPtr UIModule => (IntPtr) properties.First(i => i.Name.Equals("UIModule")).GetValue(null);

        public static IntPtr RaptureAtkModule => (IntPtr) properties.First(i => i.Name.Equals("RaptureAtkModule")).GetValue(null);

        public static IntPtr RaptureShellModule => (IntPtr) properties.First(i => i.Name.Equals("RaptureShellModule")).GetValue(null);

        public static IntPtr RaptureTeleportHistory => (IntPtr) properties.First(i => i.Name.Equals("RaptureTeleportHistory")).GetValue(null);

        public static IntPtr RaptureLogModule => (IntPtr) properties.First(i => i.Name.Equals("RaptureLogModule")).GetValue(null);

        public static IntPtr RaptureGearsetModule => (IntPtr) properties.First(i => i.Name.Equals("RaptureGearsetModule")).GetValue(null);

        public static IntPtr RaptureHotbarModule => (IntPtr) properties.First(i => i.Name.Equals("RaptureHotbarModule")).GetValue(null);

        public static IntPtr PronounModule => (IntPtr) properties.First(i => i.Name.Equals("PronounModule")).GetValue(null);

        public static IntPtr UIInputModule => (IntPtr) properties.First(i => i.Name.Equals("UIInputModule")).GetValue(null);

        public static IntPtr UIInputModule_Topic => (IntPtr) properties.First(i => i.Name.Equals("UIInputModule_Topic")).GetValue(null);
    }
}