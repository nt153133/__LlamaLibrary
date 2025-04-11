using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers
{
    public static class ScreenshotHelper
    {
        private static class Offsets
        {
            //7.1
            [Offset("Search E8 ? ? ? ? 84 C0 75 ? C6 05 ? ? ? ? ? E8 ? ? ? ? 48 89 05 ? ? ? ? Add 1 TraceRelative")]
            //[OffsetCN("Search E8 ? ? ? ? 84 C0 75 19 F3 0F 10 05 ? ? ? ? Add 1 TraceRelative")]
            internal static IntPtr ScreenshotFunc;

            [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? BB ? ? ? ? 83 FA ?")]
            internal static IntPtr CallbackFunction;

            [Offset("Search 48 8B 0D ? ? ? ? 48 8D 15 ? ? ? ? 45 33 C0 E8 ? ? ? ? 84 C0 Add 3 TraceRelative")]
            internal static IntPtr ScreenshotStruct;

            //7.1
            [Offset("Search C6 05 ? ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? Add 2 TraceRelative")]
            //[OffsetCN("Search C6 05 ? ? ? ? ? 48 83 C4 ? 5F C3 48 8B 4F ? Add 2 TraceRelative")]
            internal static IntPtr ScreenshotState;

            [Offset("Search 48 8D 4B ? 48 8D 44 24 ? 48 3B C1 Add 3 Read8")]
            internal static int Filename;

            [Offset("Search C6 43 ? ? B0 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 81 EC ? ? ? ? Add 2 Read16")]
            internal static int Busy;

            //7.1
            //TODO: Update this offset
            /*[Offset("Search F3 0F 10 15 ? ? ? ? 0F 57 C0 0F 2F D0 Add 4 TraceRelative")]
            [OffsetCN("Search F3 0F 10 15 ? ? ? ? 0F 57 C0 0F 2F D0 Add 4 TraceRelative")]
            internal static IntPtr FloatThing;*/
        }

        public static IntPtr ScreenshotStruct => Core.Memory.Read<IntPtr>(Offsets.ScreenshotStruct);

        public static string LastFilename => Core.Memory.ReadStringW(ScreenshotStruct + Offsets.Filename);

        public static byte State => Core.Memory.Read<byte>(Offsets.ScreenshotState);

        public static byte Busy => Core.Memory.Read<byte>(ScreenshotStruct + Offsets.Busy);

        public static bool CallScreenshotRaw()
        {
            return Core.Memory.CallInjectedWraper<bool>(
                Offsets.ScreenshotFunc,
                ScreenshotStruct,
                Offsets.CallbackFunction,
                0);
        }

        public static async Task<string> TakeScreenshot()
        {
            CallScreenshotRaw();
            await Coroutine.Wait(5000, () => Busy != 0);
            await Coroutine.Wait(5000, () => Busy == 0);
            return LastFilename;
        }
    }
}