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
    /// Provides detailed information about a specific housing plot, including its sale status, size, and lottery details.
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
        /// Gets the TerritoryType ID of the housing zone.
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
        /// Gets the size of the housing plot (Small, Medium, or Large).
        /// </summary>
        public PlotSize Size => (PlotSize)Core.Memory.Read<byte>(Pointer + AgentHousingSignBoardOffsets.Size);

        /// <summary>
        /// Gets the winning lottery number for the most recent drawing on this plot.
        /// </summary>
        /// <remarks>
        /// Implementation note: This property reads from an offset 0xC bytes after <c>LotteryEntryCount</c>.
        /// </remarks>
        public ushort WinningLotteryNumber => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.LotteryEntryCount + 0xC);

        /// <summary>
        /// Gets the total number of lottery entries submitted for this plot.
        /// </summary>
        /// <remarks>
        /// Implementation note: This property reads from the <c>WinningLotteryNumber</c> offset.
        /// </remarks>
        public ushort LotteryEntryCount => Core.Memory.Read<ushort>(Pointer + AgentHousingSignBoardOffsets.WinningLotteryNumber);

        /// <summary>
        /// Gets a value indicating whether the plot is owned by a Free Company.
        /// </summary>
        public bool FcOwned => Core.Memory.Read<int>(Pointer + AgentHousingSignBoardOffsets.FcOwned) != 0;

        /// <summary>
        /// Returns a string that represents the current housing plot status.
        /// </summary>
        /// <returns>A formatted string containing zone, ward, plot, sale status, size, and lottery data.</returns>
        public override string ToString()
        {
            return $"Zone: {Zone}, Ward: {Ward}, Plot: {Plot}, ForSale: {ForSale}, Size: {Size}, LotteryEntryCount: {LotteryEntryCount}, WinningLotteryNumber: {WinningLotteryNumber}, FcOwned: {FcOwned}";
        }
    }
}