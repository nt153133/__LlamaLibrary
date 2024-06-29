#if RB_DT
using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Managers
{
    public class PointMenuManager
    {
        private static LLogger Log = new LLogger("PointMenu", Colors.Silver);
        internal static class Offsets
        {
            [OffsetDawntrail("Search 48 8B 89 ? ? ? ? 49 8B E9 41 8B F0 Add 3 Read32")]
            internal static int ObjectCount;

        }

        public static AtkAddonControl Window => RaptureAtkUnitManager.GetWindowByName("PointMenu");

        public static NativeVectorV2<IntPtr> Objects => Core.Memory.Read<NativeVectorV2<IntPtr>>(Window.Pointer + Offsets.ObjectCount);

        public static void Interact(ulong position)
        {
            Log.Information($"Clicking on {position}");

            using (var mem = Core.Memory.CreateAllocatedMemory(256))
            {
                var func = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Window.Pointer) + (8 * 2));

                Core.Memory.CallInjected64<IntPtr>(func, Window.Pointer, 25UL, position, mem.Address);
            }
        }
    }
}
#endif