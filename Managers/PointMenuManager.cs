#if RB_DT
using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Managers
{
    public class PointMenuManager
    {
        private static LLogger Log = new LLogger("PointMenu", Colors.Silver);
        

        public static AtkAddonControl Window => RaptureAtkUnitManager.GetWindowByName("PointMenu");

        public static NativeVectorV2<IntPtr> Objects => Core.Memory.Read<NativeVectorV2<IntPtr>>(Window.Pointer + PointMenuManagerOffsets.ObjectCount);

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