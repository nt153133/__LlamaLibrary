using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the house or plot signboard interface.
    /// </summary>
    public class AgentHousingSignboard : AgentInterface<AgentHousingSignboard>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentHousingSignBoardOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHousingSignboard"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentHousingSignboard(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the territory ID of the housing zone.
        /// </summary>
        public ushort Zone => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.Zone);

        /// <summary>
        /// Gets the 1-based ward number of the plot.
        /// </summary>
        public byte Ward => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Ward) + 1);

        /// <summary>
        /// Gets the 1-based plot number of the plot.
        /// </summary>
        public byte Plot => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Plot) + 1);

        /// <summary>
        /// Gets a value indicating whether the plot is currently for sale.
        /// </summary>
        public bool ForSale => Core.Memory.Read<bool>(Pointer + AgentHousingSignBoardOffsets.ForSale);

        /// <summary>
        /// Gets the size of the plot (Small, Medium, or Large).
        /// </summary>
        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Size);

        /// <summary>
        /// Gets the winning number for the most recent lottery cycle for this plot.
        /// </summary>
        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.LotteryEntryCount + 0xC);

        /// <summary>
        /// Gets the total number of entries in the current or most recent lottery cycle.
        /// </summary>
        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.WinningLotteryNumber);

        /// <summary>
        /// Gets a value indicating whether the plot is currently owned by a Free Company.
        /// </summary>
        public bool FcOwned => Core.Memory.Read<int>(Pointer + AgentHousingSignBoardOffsets.FcOwned) != 0;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}