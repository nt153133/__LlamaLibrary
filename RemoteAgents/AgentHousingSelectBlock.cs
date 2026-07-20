using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing district ward/block selection interface.
    /// Manages the state and interactions of selecting wards and viewing individual plot availability status.
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
        /// Gets or sets the currently selected ward number.
        /// </summary>
        /// <value>
        /// The zero-based index of the ward (e.g., 0 represents Ward 1).
        /// </value>
        public int WardNumber
        {
            get => Core.Memory.Read<int>(Pointer + AgentHousingSelectBlockOffsets.WardNumber);
            set => Core.Memory.Write(Pointer + AgentHousingSelectBlockOffsets.WardNumber, value);
        }

        /// <summary>
        /// Reads the status of housing plots for the currently selected ward from game memory.
        /// </summary>
        /// <param name="count">The number of plots to read status for (usually matching the total plots in a ward, e.g., 30 or 60).</param>
        /// <returns>A byte array where each byte represents the availability or occupancy status of a plot in the ward.</returns>
        public byte[] ReadPlots(int count)
        {
            return Core.Memory.ReadArray<byte>(Pointer + AgentHousingSelectBlockOffsets.PlotOffset, count);
        }
    }
}
