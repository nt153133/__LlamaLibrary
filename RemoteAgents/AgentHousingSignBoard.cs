using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing signboard interface.
    /// Provides detailed information about a specific housing plot, including its sale status, ward, plot number, and lottery data.
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
        /// Gets the zone ID (TerritoryType) where the plot is located.
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
        /// Gets a value indicating whether the plot is currently available for purchase.
        /// </summary>
        public bool ForSale => Core.Memory.Read<bool>(Pointer + AgentHousingSignBoardOffsets.ForSale);

        /// <summary>
        /// Gets the size category of the plot (Small, Medium, or Large).
        /// </summary>
        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Size);

        /// <summary>
        /// Gets the number of the winning ticket for the most recent lottery on this plot.
        /// </summary>
        /// <remarks>
        /// Note: This property currently utilizes the offset named <c>LotteryEntryCount</c> (with an adjustment) from the offset manager.
        /// </remarks>
        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.LotteryEntryCount + 0xC);

        /// <summary>
        /// Gets the total number of entries submitted for the current or most recent lottery on this plot.
        /// </summary>
        /// <remarks>
        /// Note: This property currently utilizes the offset named <c>WinningLotteryNumber</c> from the offset manager.
        /// </remarks>
        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.WinningLotteryNumber);

        /// <summary>
        /// Gets a value indicating whether the plot is owned by a Free Company.
        /// </summary>
        public bool FcOwned => Core.Memory.Read<int>(Pointer + AgentHousingSignBoardOffsets.FcOwned) != 0;

        /// <summary>
        /// Returns a formatted string describing the plot's current status and location.
        /// </summary>
        /// <returns>A summary string containing zone, ward, plot, sale status, size, and lottery details.</returns>
        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}