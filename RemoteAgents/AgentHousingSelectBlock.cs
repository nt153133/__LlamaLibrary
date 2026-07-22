using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing ward and plot selection interface.
    /// Manages the visual block list of housing wards, allowing inspection and updates to ward and plot selection states.
    /// </summary>
    public class AgentHousingSelectBlock : AgentInterface<AgentHousingSelectBlock>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentHousingSelectBlockOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHousingSelectBlock"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentHousingSelectBlock(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets or sets the selected ward index.
        /// </summary>
        /// <value>The zero-based index of the ward (e.g., 0 represents Ward 1, 1 represents Ward 2, etc.).</value>
        public int WardNumber
        {
            get => Core.Memory.Read<int>(Pointer + AgentHousingSelectBlockOffsets.WardNumber);
            set => Core.Memory.Write(Pointer + AgentHousingSelectBlockOffsets.WardNumber, value);
        }

        /// <summary>
        /// Reads the status or availability of the plots in the currently selected ward from game memory.
        /// </summary>
        /// <param name="count">The number of plots to read status/availability data for (typically corresponding to the total number of plots in the residential zone, e.g., 30 or 60).</param>
        /// <returns>A byte array where each element represents the status or availability code of the respective plot in the ward.</returns>
        public byte[] ReadPlots(int count)
        {
            return Core.Memory.ReadArray<byte>(Pointer + AgentHousingSelectBlockOffsets.PlotOffset, count);
        }
    }
}
