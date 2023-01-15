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
            internal static IntPtr Vtable;

            //6.3 0x58
            [Offset("Search 4C 8B 47 ? 0F B6 44 4F ? Add 3 Read8")]
            internal static int InfoOffset;

            //0x28
            [Offset("Search 49 8B 40 ? 49 8B C8 48 8B 1C D0 Add 3 Read8")]
            internal static int StartingPointer;

            //0x30
            [Offset("Search 49 8B 40 ? 49 2B 40 ? 48 C1 F8 ? 8B DA Add 3 Read8")]
            internal static int EndingPointer;
        }

        protected AgentFishGuide2(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + Offsets.InfoOffset);

        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + Offsets.StartingPointer);

        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + Offsets.EndingPointer);

        public int SlotCount => (int)((EndingPointer.ToInt64() - StartingPointer.ToInt64()) / 8);

        public FishGuide2Item[] GetFishListRaw()
        {
            return Core.Memory.ReadArray<FishGuide2Item>(Core.Memory.Read<IntPtr>(StartingPointer), SlotCount); //.Select(x => x.FishItem) as List<uint>;
        }

        public async Task<FishGuide2Item[]> GetFishList()
        {
            if (!FishGuide2.Instance.IsOpen)
            {
                this.Toggle();
                if (!await Coroutine.Wait(10000, () => FishGuide2.Instance.IsOpen))
                {
                    return Array.Empty<FishGuide2Item>();
                }
            }

            FishGuide2.Instance.SelectFishing();

            await Coroutine.Sleep(2000);

            var results = GetFishListRaw();

            FishGuide2.Instance.Close();
            await Coroutine.Wait(10000, () => !FishGuide2.Instance.IsOpen);

            return results;
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct FishGuide2Item
    {
        public uint FishItem;
        public ushort Index;
        public ushort Unknown;
        public ushort Unknown2;
        public bool HasCaught;
        public byte Unknown3;
        public ushort Unknown4;
        public ushort Unknown5;

        public override string ToString()
        {
            return $"{DataManager.GetItem(FishItem)} : {HasCaught}";
        }
    }
}