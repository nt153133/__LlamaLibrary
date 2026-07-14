using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Housing Signboard interface.
    /// Provides details about a specific housing plot, including its availability and lottery status.
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
        /// Gets the zone ID where the housing plot is located.
        /// </summary>
        public ushort Zone => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.Zone);

        /// <summary>
        /// Gets the 1-based ward number of the plot.
        /// </summary>
        public byte Ward => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Ward) + 1);

        /// <summary>
        /// Gets the 1-based plot number.
        /// </summary>
        public byte Plot => (byte)(Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Plot) + 1);

        /// <summary>
        /// Gets a value indicating whether the plot is currently for sale.
        /// </summary>
        public bool ForSale => Core.Memory.Read<bool>(Pointer + AgentHousingSignBoardOffsets.ForSale);

        /// <summary>
        /// Gets the size category of the plot (Small, Medium, or Large).
        /// </summary>
        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Size);

        /// <summary>
        /// Gets the winning lottery number for the current or most recent lottery.
        /// </summary>
        /// <remarks>
        /// Implementation Note: This property currently uses an offset relative to <c>LotteryEntryCount</c> from <see cref="AgentHousingSignBoardOffsets"/>.
        /// </remarks>
        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.LotteryEntryCount + 0xC);

        /// <summary>
        /// Gets the total number of entries in the lottery for this plot.
        /// </summary>
        /// <remarks>
        /// Implementation Note: This property currently uses the <c>WinningLotteryNumber</c> offset from <see cref="AgentHousingSignBoardOffsets"/>.
        /// </remarks>
        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.WinningLotteryNumber);

        /// <summary>
        /// Gets a value indicating whether the plot is owned by a Free Company.
        /// </summary>
        public bool FcOwned => Core.Memory.Read<int>(Pointer + AgentHousingSignBoardOffsets.FcOwned) != 0;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}