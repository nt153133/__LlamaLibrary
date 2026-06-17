#if RB_DT
using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;

namespace LlamaLibrary.LlamaManagers
{
    /// <summary>
    /// Manages interactions with the 'PointMenu' UI window, primarily used for investigation and point-and-click
    /// mechanics introduced in the Dawntrail expansion.
    /// </summary>
    public class PointMenuManager
    {
        private static LLogger Log = new LLogger("PointMenu", Colors.Silver);

        /// <summary>
        /// Gets the <see cref="AtkAddonControl"/> for the 'PointMenu' window.
        /// </summary>
        public static AtkAddonControl Window => RaptureAtkUnitManager.GetWindowByName("PointMenu");

        /// <summary>
        /// Gets a collection of memory pointers to the interactive objects currently managed by the PointMenu.
        /// </summary>
        public static NativeVectorV2<IntPtr> Objects => Core.Memory.Read<NativeVectorV2<IntPtr>>(Window.Pointer + PointMenuManagerOffsets.ObjectCount);

        /// <summary>
        /// Simulates a user interaction (click) on a specific element within the PointMenu.
        /// </summary>
        /// <param name="position">The index or identifier of the object to interact with.</param>
        public static void Interact(ulong position)
        {
            Log.Information($"Clicking on {position}");

            Window.SendAction(2, 0x3, 0xd, 0x5, position);

            //using (var mem = Core.Memory.CreateAllocatedMemory(256))
            //{
            //    var func = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Window.Pointer) + (8 * 2));

            //    Core.Memory.CallInjectedWraper<IntPtr>(func, Window.Pointer, 25UL, position, mem.Address);
            //}
        }
    }
}
#endif