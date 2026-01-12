using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFreeCompanyChest : AgentInterface<AgentFreeCompanyChest>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentFreeCompanyChest;

        

        public byte SelectedTabIndex => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.SelectedTabIndex);

        public bool CrystalsTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.CrystalsTabSelected);

        public bool GilTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.GilTabSelected);

        public byte GilWithdrawDeposit => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.GilWithdrawDeposit);

        public uint GilAmountTransfer
        {
            get => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer);
            set => Core.Memory.Write(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer, value);
        }

        public uint GilBalance => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilCount);

        public bool FullyLoaded => Core.Memory.NoCacheRead<bool>(Pointer + AgentFreeCompanyChestOffsets.FullyLoaded);

        public byte LoadBagCall(InventoryBagId bagId)
        {
            return Core.Memory.CallInjectedWraper<byte>(AgentFreeCompanyChestOffsets.BagRequestCall, Offsets.g_InventoryManager, bagId);
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