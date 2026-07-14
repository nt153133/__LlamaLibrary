using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Free Company Chest interface.
    /// Manages synchronization of chest contents, gil transfers, and tab selection.
    /// </summary>
    public class AgentFreeCompanyChest : AgentInterface<AgentFreeCompanyChest>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFreeCompanyChestOffsets.VTable;

        /// <summary>
        /// Gets the index of the currently selected item tab in the Free Company Chest.
        /// </summary>
        public byte SelectedTabIndex => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.SelectedTabIndex);

        /// <summary>
        /// Gets a value indicating whether the Crystals tab is currently selected.
        /// </summary>
        public bool CrystalsTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.CrystalsTabSelected);

        /// <summary>
        /// Gets a value indicating whether the Gil tab is currently selected.
        /// </summary>
        public bool GilTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.GilTabSelected);

        /// <summary>
        /// Gets the state of the gil transfer interface (Withdraw or Deposit).
        /// </summary>
        public byte GilWithdrawDeposit => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.GilWithdrawDeposit);

        /// <summary>
        /// Gets or sets the amount of gil to be transferred in the Gil tab.
        /// </summary>
        public uint GilAmountTransfer
        {
            get => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer);
            set => Core.Memory.Write(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer, value);
        }

        /// <summary>
        /// Gets the total balance of gil currently stored in the Free Company Chest.
        /// </summary>
        public uint GilBalance => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilCount);

        /// <summary>
        /// Gets a value indicating whether the chest data has been fully loaded and synchronized from the server.
        /// </summary>
        public bool FullyLoaded => Core.Memory.NoCacheRead<bool>(Pointer + AgentFreeCompanyChestOffsets.FullyLoaded);

        /// <summary>
        /// Calls the internal game function to request the contents of a specific inventory bag (chest tab).
        /// </summary>
        /// <param name="bagId">The <see cref="InventoryBagId"/> of the tab to request.</param>
        /// <returns>A byte indicating the result of the call (typically 1 for success).</returns>
        public byte LoadBagCall(InventoryBagId bagId)
        {
            return Core.Memory.CallInjectedWraper<byte>(AgentFreeCompanyChestOffsets.BagRequestCall, Offsets.g_InventoryManager, bagId);
        }

        /// <summary>
        /// Asynchronously requests the contents of a specific chest tab and waits for it to be fully loaded.
        /// </summary>
        /// <param name="bagId">The <see cref="InventoryBagId"/> of the tab to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadBag(InventoryBagId bagId)
        {
            var result = LoadBagCall(bagId) == 1;

            if (result)
            {
                await Coroutine.Wait(5000, () => !FullyLoaded);
                await Coroutine.Wait(5000, () => FullyLoaded);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentFreeCompanyChest"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentFreeCompanyChest(IntPtr pointer) : base(pointer)
        {
        }
    }
}