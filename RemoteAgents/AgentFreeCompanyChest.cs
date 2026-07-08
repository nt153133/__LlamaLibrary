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
    /// Remote agent for the Free Company chest interface.
    /// Manages the state and logic for interacting with the FC chest, including item tabs, crystals, and gil.
    /// </summary>
    public class AgentFreeCompanyChest : AgentInterface<AgentFreeCompanyChest>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFreeCompanyChestOffsets.VTable;

        /// <summary>
        /// Gets the zero-based index of the currently selected item tab in the chest.
        /// </summary>
        public byte SelectedTabIndex => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.SelectedTabIndex);

        /// <summary>
        /// Gets a value indicating whether the crystals tab is currently selected.
        /// </summary>
        public bool CrystalsTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.CrystalsTabSelected);

        /// <summary>
        /// Gets a value indicating whether the gil tab is currently selected.
        /// </summary>
        public bool GilTabSelected => Core.Memory.Read<bool>(Pointer + AgentFreeCompanyChestOffsets.GilTabSelected);

        /// <summary>
        /// Gets the current gil transfer mode (e.g., withdraw or deposit).
        /// </summary>
        public byte GilWithdrawDeposit => Core.Memory.Read<byte>(Pointer + AgentFreeCompanyChestOffsets.GilWithdrawDeposit);

        /// <summary>
        /// Gets or sets the amount of gil to be transferred (withdrawn or deposit).
        /// </summary>
        public uint GilAmountTransfer
        {
            get => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer);
            set => Core.Memory.Write(Pointer + AgentFreeCompanyChestOffsets.GilAmountTransfer, value);
        }

        /// <summary>
        /// Gets the total amount of gil currently stored in the Free Company chest.
        /// </summary>
        public uint GilBalance => Core.Memory.Read<uint>(Pointer + AgentFreeCompanyChestOffsets.GilCount);

        /// <summary>
        /// Gets a value indicating whether the chest data has been fully synchronized and loaded from the server.
        /// </summary>
        public bool FullyLoaded => Core.Memory.NoCacheRead<bool>(Pointer + AgentFreeCompanyChestOffsets.FullyLoaded);

        /// <summary>
        /// Sends a request to the game server to load data for a specific inventory bag.
        /// </summary>
        /// <param name="bagId">The identifier of the inventory bag to load.</param>
        /// <returns>A byte value indicating the result of the call (1 for success).</returns>
        public byte LoadBagCall(InventoryBagId bagId)
        {
            return Core.Memory.CallInjectedWraper<byte>(AgentFreeCompanyChestOffsets.BagRequestCall, Offsets.g_InventoryManager, bagId);
        }

        /// <summary>
        /// Asynchronously requests data for a specific bag and waits for the chest to be fully loaded.
        /// </summary>
        /// <param name="bagId">The identifier of the inventory bag to load.</param>
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