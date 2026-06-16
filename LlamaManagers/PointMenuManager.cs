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
    /// Static manager for interacting with the "PointMenu" UI window, used in Dawntrail investigation sequences.
    /// Provides access to the interactive objects within the menu and executes interactions.
    /// </summary>
    public class PointMenuManager
    {
        private static LLogger Log = new LLogger("PointMenu", Colors.Silver);

        /// <summary>
        /// Gets the <see cref="AtkAddonControl"/> for the "PointMenu" window if it is open.
        /// </summary>
        public static AtkAddonControl Window => RaptureAtkUnitManager.GetWindowByName("PointMenu");

        /// <summary>
        /// Gets the collection of interactive object pointers currently present in the PointMenu.
        /// </summary>
        public static NativeVectorV2<IntPtr> Objects => Core.Memory.Read<NativeVectorV2<IntPtr>>(Window.Pointer + PointMenuManagerOffsets.ObjectCount);

        /// <summary>
        /// Interacts with an object in the PointMenu at the specified <paramref name="position"/>.
        /// Sends a specific UI action to the window to trigger the interaction.
        /// </summary>
        /// <param name="position">The zero-based index or position of the object in the menu to interact with.</param>
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
