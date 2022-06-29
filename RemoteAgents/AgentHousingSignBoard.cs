using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHousingSignboard : AgentInterface<AgentHousingSignboard>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 48 8D 4B ? 66 89 7B ? 89 7B ? Add 3 TraceRelative")]
            internal static IntPtr VTable;

            [Offset("Search 40 88 7B ? 44 88 63 ? Add 3 Read8")]
            internal static int Ward;

            [Offset("44 88 63 ? 66 44 89 43 ? Add 3 Read8")]
            internal static int Plot;

            [Offset("66 44 89 73 ? 40 88 7B ? Add 4 Read8")]
            internal static int Zone;

            [Offset("40 88 7B ? 88 43 ? Add 3 Read8")]
            internal static int ForSale;

            [Offset("88 43 ? E8 ? ? ? ? 48 8B 4B ? Add 2 Read8")]
            internal static int Size;

            [Offset("0F 11 4B ? 41 80 7D ? ? Add 3 Read8")]
            internal static int WinningLotteryNumber;

            [Offset("0F 11 43 ? 41 0F 10 4D ? Add 3 Read8")]
            internal static int LotteryEntryCount;

            [Offset("49 89 87 ? ? ? ? 48 8B 01 Add 3 Read32")]
            internal static int FcOwned;
        }

        protected AgentHousingSignboard(IntPtr pointer) : base(pointer)
        {
        }

        public ushort Zone => Core.Memory.Read<ushort>(Pointer + Offsets.Zone);

        public byte Ward => (byte)(Core.Memory.Read<byte>(Pointer + Offsets.Ward) + 1);

        public byte Plot => (byte)(Core.Memory.Read<byte>(Pointer + Offsets.Plot) + 1);

        public bool ForSale => Core.Memory.Read<bool>(Pointer + Offsets.ForSale);

        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + Offsets.Size);

        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + Offsets.LotteryEntryCount + 0xC);

        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + Offsets.WinningLotteryNumber);

        public bool FcOwned => Core.Memory.Read<int>(Pointer + Offsets.FcOwned) != 0;

        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}