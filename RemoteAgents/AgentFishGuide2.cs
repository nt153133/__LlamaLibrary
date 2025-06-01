using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.Vtable;

        public const int TabCount = 37;

        private static class Offsets
        {
            //6.3
            [Offset("Search 48 8D 05 ? ? ? ? 33 C9 48 89 03 BA ? ? ? ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 89 07 48 8D 4F ? 33 C0 48 89 77 ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //6.3 0x58
            [Offset("Search 4C 8B 47 ? 0F B6 44 4F ? Add 3 Read8")]
            [OffsetDawntrail("Search 48 8B 4B ? 44 8B C7 48 8B 41 ? Add 3 Read8")]
            internal static int InfoOffset;

            //0x28
            [Offset("Search 49 8B 40 ? 49 8B C8 48 8B 1C D0 Add 3 Read8")]
            [OffsetDawntrail("Search 48 8B 41 ? 48 8B 51 ? 48 2B D0 Add 3 Read8")]
            internal static int StartingPointer;

            //0x30
            [Offset("Search 49 8B 40 ? 49 2B 40 ? 48 C1 F8 ? 8B DA Add 3 Read8")]
            [OffsetDawntrail("Search 48 8B 51 ? 48 2B D0 48 C1 FA ? 4C 3B C2 Add 3 Read8")]
            internal static int EndingPointer;
        }

        protected AgentFishGuide2(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + Offsets.InfoOffset);

        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + Offsets.StartingPointer);

        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + Offsets.EndingPointer);

        public IntPtr FishListPointer(int start = 0) => Core.Memory.Read<IntPtr>(StartingPointer + (8 * start));

        public int SlotCount => (int)((EndingPointer.ToInt64() - StartingPointer.ToInt64()) / 8);

        public FishGuide2Item[] GetFishListRaw()
        {
            return Core.Memory.ReadArray<FishGuide2Item>(Core.Memory.Read<IntPtr>(StartingPointer), SlotCount); //.Select(x => x.FishItem) as List<uint>;
        }

        public FishGuide2Item[] GetFishListRaw(int start, int count)
        {
            return Core.Memory.ReadArray<FishGuide2Item>(FishListPointer(start), count); //.Select(x => x.FishItem) as List<uint>;
        }

        public async Task<FishGuide2Item[]> GetFishList(int start = 0, int count = 0)
        {
            if (!FishGuide2.Instance.IsOpen)
            {
                Toggle();
                if (!await Coroutine.Wait(10000, () => FishGuide2.Instance.IsOpen))
                {
                    return Array.Empty<FishGuide2Item>();
                }
            }

            if (count == 0)
            {
                count = SlotCount;
            }

            FishGuide2.Instance.SelectFishing();

            await Coroutine.Sleep(2000);

            var results = GetFishListRaw(start, count);

            FishGuide2.Instance.Close();
            await Coroutine.Wait(10000, () => !FishGuide2.Instance.IsOpen);

            return results;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct FishGuide2Item
    {
        //0
        [FieldOffset(0x0)]
        public uint FishItem;

        //0x4
        [FieldOffset(0X4)]
        public ushort Index;

        //0x6
        [FieldOffset(0x6)]
        public ushort Unknown;

        //0x8
        [FieldOffset(0x8)]
        public ushort Unknown2;

        //0xA
        [FieldOffset(0xA)]
        public ushort Unknown5;

        //0xE
        [FieldOffset(0xE)]
        public byte bHasCaught;

        public bool HasCaught => bHasCaught == 1;

        public override string ToString()
        {
            return $"{DataManager.GetItem(FishItem)} : {HasCaught}";
        }
    }
}