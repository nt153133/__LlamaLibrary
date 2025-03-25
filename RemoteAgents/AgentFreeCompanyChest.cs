using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFreeCompanyChest : AgentInterface<AgentFreeCompanyChest>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? ? ? ? 48 8D 59 ? 48 8D 05 ? ? ? ? 48 89 51 ? 48 89 41 ? 8D 75 Add 3 TraceRelative")]
            [OffsetCN("Search 48 8D 05 ? ? ? ? 48 89 51 ? 48 89 01 48 8D 59 ? 48 8D 05 ? ? ? ? 48 89 69 ? 48 89 41 ? 8D 75 ? Add 3 TraceRelative")]
            internal static IntPtr VTable;

            [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 8B F2 48 8B D9 33 D2 0F B7 FA")]
            [OffsetDawntrail("Search E8 ? ? ? ? 8B 8B ? ? ? ? 84 C0 74 ? 83 C9 ? 89 8B ? ? ? ? 48 83 C4 ? 5B C3 F6 C1 ? 0F B6 C0 Add 1 TraceRelative")]
            internal static IntPtr BagRequestCall;

            [Offset("Search 0F B6 7B ? BA ? ? ? ? E8 ? ? ? ? BA ? ? ? ? 89 7C 24 ? Add 3 Read8")]
            internal static int SelectedTabIndex;

            [Offset("Search 0F B6 7B ? 48 8D 4C 24 ? BA ? ? ? ? C7 44 24 ? ? ? ? ? E8 ? ? ? ? 89 7C 24 ? 48 8D 4C 24 ? 0F B6 7B ? Add 3 Read8")]
            internal static int CrystalsTabSelected;

            [Offset("Search 89 83 ? ? ? ? EB ? 48 8B 4B ? 48 8B 01 FF 50 ? 8B 93 ? ? ? ? Add 2 Read32")]
            internal static int GilTabSelected;

            [Offset("Search 88 83 ? ? ? ? EB ? 48 8D 4E ? Add 2 Read32")]
            internal static int GilWithdrawDeposit;

            [Offset("Search 89 83 ? ? ? ? 48 8B CB E8 ? ? ? ? 48 8B 5C 24 ? 40 0F B6 C7 Add 3 Read32")]
            internal static int GilAmountTransfer;

            [Offset("Search 89 BB ? ? ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 8B 5C 24 ? Add 2 Read32")]
            internal static int GilCount;

            [Offset("Search 88 83 ? ? ? ? E8 ? ? ? ? E9 ? ? ? ? 48 8B 4B ? Add 2 Read32")]
            [OffsetDawntrail("Search 38 83 ? ? ? ? 0F 84 ? ? ? ? 88 83 ? ? ? ? 48 8B CB Add 2 Read32")]
            internal static int FullyLoaded;
        }

        public byte SelectedTabIndex => Core.Memory.Read<byte>(Pointer + Offsets.SelectedTabIndex);

        public bool CrystalsTabSelected => Core.Memory.Read<bool>(Pointer + Offsets.CrystalsTabSelected);

        public bool GilTabSelected => Core.Memory.Read<bool>(Pointer + Offsets.GilTabSelected);

        public byte GilWithdrawDeposit => Core.Memory.Read<byte>(Pointer + Offsets.GilWithdrawDeposit);

        public uint GilAmountTransfer
        {
            get => Core.Memory.Read<uint>(Pointer + Offsets.GilAmountTransfer);
            set => Core.Memory.Write(Pointer + Offsets.GilAmountTransfer, value);
        }

        public uint GilBalance => Core.Memory.Read<uint>(Pointer + Offsets.GilCount);

        public bool FullyLoaded => Core.Memory.NoCacheRead<bool>(Pointer + Offsets.FullyLoaded);

        public byte LoadBagCall(InventoryBagId bagId)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                return Core.Memory.CallInjected64<byte>(Offsets.BagRequestCall, Memory.Offsets.g_InventoryManager, bagId);
            }
        }

        public async Task LoadBag(InventoryBagId bagId)
        {
            var result = LoadBagCall(bagId) == 1;

            if (result)
            {
                await Coroutine.Wait(5000, () => !FullyLoaded);
                await Coroutine.Wait(5000, () => FullyLoaded);
            }
        }

        protected AgentFreeCompanyChest(IntPtr pointer) : base(pointer)
        {
        }
    }
}