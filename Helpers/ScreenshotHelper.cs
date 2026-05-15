using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Wraps the game's screenshot capture pipeline to programmatically take screenshots
    /// and retrieve the resulting file path.
    /// </summary>
    public static class ScreenshotHelper
    {
        

        /// <summary>Gets the base pointer to the game's screenshot manager struct in memory.</summary>
        public static IntPtr ScreenshotStruct => Core.Memory.Read<IntPtr>(ScreenshotHelperOffsets.ScreenshotStruct);

        /// <summary>Gets the file path of the last screenshot taken (wide-char string from game memory).</summary>
        public static string LastFilename => Core.Memory.ReadStringW(ScreenshotStruct + ScreenshotHelperOffsets.Filename);

        /// <summary>Gets the current screenshot capture state byte from game memory.</summary>
        public static byte State => Core.Memory.Read<byte>(ScreenshotHelperOffsets.ScreenshotState);

        /// <summary>Gets the screenshot busy flag; non-zero while a capture is in progress.</summary>
        public static byte Busy => Core.Memory.Read<byte>(ScreenshotStruct + ScreenshotHelperOffsets.Busy);

        /// <summary>
        /// Calls the game's internal screenshot function directly via an injected call, without waiting for completion.
        /// </summary>
        /// <returns><see langword="true"/> if the call succeeded.</returns>
        public static bool CallScreenshotRaw()
        {
            return Core.Memory.CallInjectedWraper<bool>(
                ScreenshotHelperOffsets.ScreenshotFunc,
                ScreenshotStruct,
                ScreenshotHelperOffsets.CallbackFunction,
                0);
        }

        /// <summary>
        /// Takes a screenshot and waits up to 5 seconds for the capture to begin and then complete.
        /// </summary>
        /// <returns>The file path of the saved screenshot image.</returns>
        public static async Task<string> TakeScreenshot()
        {
            CallScreenshotRaw();
            await Coroutine.Wait(5000, () => Busy != 0);
            await Coroutine.Wait(5000, () => Busy == 0);
            return LastFilename;
        }
    }
}