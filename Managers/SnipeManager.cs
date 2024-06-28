using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Managers
{
    public class SnipeManager
    {
        private static LLogger Log = new LLogger("SnipeManager", Colors.Silver);

        internal static class Offsets
        {
            [Offset("Search 48 8D 0D ? ? ? ? 32 DB E8 ? ? ? ? 84 C0 0F B6 CB BA ? ? ? ? 0F 45 CA 45 32 FF Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 74 05 45 32 E4 EB 0F Add 3 TraceRelative")]
            internal static IntPtr Instance;

            [Offset("Search 8B 83 ? ? ? ? 48 8B 54 24 ? 48 89 4D ? Add 2 Read32")]
            [OffsetDawntrail("Search 8B 83 ? ? ? ? 89 45 10 48 8D 45 10 48 89 45 18 48 8D 42 01 48 3B C8 77 17 41 8B D6 48 8D 4C 24 ? E8 ? ? ? ? 48 8B 54 24 ? 48 8B 4C 24 ? Add 2 Read32")]
            internal static int Id; //0x5940

            //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ?
            [Offset("Search 0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 2 Read32")]
            internal static int Active;

            //66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32
            [Offset("Search 66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32")]
            internal static int State;

            //48 8B 8B ? ? ? ? 48 8B 0C D1 Add 3 Read32
            [Offset("Search 48 8B 8B ? ? ? ? 48 8B 0C D1 Add 3 Read32")]
            [OffsetDawntrail("Search 0F B6 93 ? ? ? ? 48 8B 83 ? ? ? ? Add 3 Read32")]
            internal static int SnipeObjects;

            //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32
            [Offset("Search 0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32")]
            internal static int Shoot;

            //0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8
            [Offset("Search 0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
            internal static int ShootParam;

            // 44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32
            [Offset("Search 44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32")]
            internal static int ShootData;

            //0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ?
            [Offset("Search 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
            internal static int ShootParam2;
        }

        public static IntPtr addr => Core.Memory.Read<IntPtr>(Offsets.Instance);
        public static uint SnipeRowId => Core.Memory.Read<uint>(addr + Offsets.Id);

        public static byte State
        {
            get => Core.Memory.NoCacheRead<byte>(addr + Offsets.State);
            set => Core.Memory.Write(addr + Offsets.State, value);
        }

        public static byte Active
        {
            get => Core.Memory.Read<byte>(addr + Offsets.Active);
            set => Core.Memory.Write(addr + Offsets.Active, value);
        }

        private static bool Shoot
        {
            get => Core.Memory.Read<byte>(addr + Offsets.Shoot) == 1;
            set => Core.Memory.Write(addr + Offsets.Shoot, value ? 1 : 0);
        }

        public static IntPtr SnipeObjectsPtr => Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects);

        public static SnipeObject[] SnipeObjects
        {
            get
            {
                if (addr == IntPtr.Zero)
                {
                    return Array.Empty<SnipeObject>();
                }

                var first = Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects);
                var end = Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects + 8);

                var count = (uint)((end.ToInt64() - first.ToInt64()) / 0x48);
                Log.Information($"{count} snipe objects found");
                return Core.Memory.ReadArray<SnipeObject>(first, (int)count);
            }
        }

        public static bool Ready => State == 4;

        public static async Task Snipe(byte index)
        {
            if (addr == IntPtr.Zero)
            {
                return;
            }

            var obj = SnipeObjects[index];

            byte shoot_1 = 0;
            if (obj.x18 != 0 && obj.x22 != 0)
            {
                shoot_1 = 0;
            }
            else if (obj.x34 != 0 && obj.x3e != 0)
            {
                shoot_1 = 1;
            }
            else
            {
                Log.Information($"Tried to snipe object but object is in an unexpected state. {obj}");
                return;
            }

            Log.Information($"Snipe {obj.GameObject?.Name ?? "Unknown Object"}");
            Core.Memory.Write<byte>(addr + Offsets.ShootData, index); //0x5000 - this should be object Index (0x4c) (0x4e)
            //ox4c - should be the object index
            //ox4e - should be the array index (0 or 1)
            // these get set during state 4
            Core.Memory.Write<byte>(addr + Offsets.ShootData + 1, shoot_1); //0x5001
            Core.Memory.Write<byte>(addr + Offsets.ShootData + 2, shoot_1 == 0 ? obj.x22 : obj.x3e); //0x5002
            Core.Memory.Write<byte>(addr + Offsets.ShootData + 4, 1); //0x5004
            State = (byte)(State + 1);

            await Coroutine.Sleep(500);
        }
    }

    [StructLayout(layoutKind: LayoutKind.Explicit, Size = 0x48)]
    public struct SnipeObject
    {
        [FieldOffset(0)]
        private IntPtr objPtr;

        // -0x10
        [FieldOffset(0x8)]
        internal byte x8;

        //Start of array 1
        [FieldOffset(0x18)]
        internal byte x18;

        [FieldOffset(0x22)]
        internal byte x22;

        //-0x10
        [FieldOffset(0x24)]
        internal byte x24;

        //start of array 2
        [FieldOffset(0x34)]
        internal byte x34;

        [FieldOffset(0x3e)]
        internal byte x3e;

        [FieldOffset(0x44)]
        public byte Hit;

        public override string ToString()
        {
            return $"{GameObject?.Name ?? "Null Object"} - Hit:{Hit:X} - {x8:X} - {x18:X}:{x22:X} or {x34:X}:{x3e:X}";
        }

        public GameObject? GameObject
        {
            get
            {
                var id = objPtr;
                return objPtr == IntPtr.Zero ? null : GameObjectManager.GameObjects.FirstOrDefault(i => i.Pointer == id);
            }
        }
    }
}