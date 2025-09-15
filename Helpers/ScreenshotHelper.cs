using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public static class ScreenshotHelper
    {
        

        public static IntPtr ScreenshotStruct => Core.Memory.Read<IntPtr>(ScreenshotHelperOffsets.ScreenshotStruct);

        public static string LastFilename => Core.Memory.ReadStringW(ScreenshotStruct + ScreenshotHelperOffsets.Filename);

        public static byte State => Core.Memory.Read<byte>(ScreenshotHelperOffsets.ScreenshotState);

        public static byte Busy => Core.Memory.Read<byte>(ScreenshotStruct + ScreenshotHelperOffsets.Busy);

        public static bool CallScreenshotRaw()
        {
            return Core.Memory.CallInjectedWraper<bool>(
                ScreenshotHelperOffsets.ScreenshotFunc,
                ScreenshotStruct,
                ScreenshotHelperOffsets.CallbackFunction,
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