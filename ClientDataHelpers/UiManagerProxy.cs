using System;
using System.Linq;
using System.Reflection;
using ff14bot.Managers;

namespace LlamaLibrary.ClientDataHelpers
{
    public static class UiManagerProxy
    {
        private static readonly PropertyInfo[] Properties;

        static UiManagerProxy()
        {
            Properties = typeof(DataManager).Assembly.GetType("ff14bot.Managers.UiManager")
                .GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

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