using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Buddy.Offsets;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFishGuide2 : AgentInterface<AgentFishGuide2>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentFishGuide2;

        public const int TabCount = 37;

        

        protected AgentFishGuide2(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr InfoPointer => Core.Memory.Read<IntPtr>(Pointer + AgentFishGuide2Offsets.InfoOffset);

        public IntPtr StartingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.StartingPointer);

        public IntPtr EndingPointer => Core.Memory.Read<IntPtr>(InfoPointer + AgentFishGuide2Offsets.EndingPointer);

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