using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing ward selection interface.
    /// Manages the selection of wards and provides data on plot availability within those wards.
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
        /// Gets or sets the 0-based index of the currently selected housing ward.
        /// </summary>
        public int WardNumber
        {
            get => Core.Memory.Read<int>(Pointer + AgentHousingSelectBlockOffsets.WardNumber);
            set => Core.Memory.Write(Pointer + AgentHousingSelectBlockOffsets.WardNumber, value);
        }

        /// <summary>
        /// Reads a byte array representing the availability and status of plots in the selected ward.
        /// </summary>
        /// <param name="count">The number of plots to read status for (typically 60).</param>
        /// <returns>A byte array where each element indicates the status of a plot.</returns>
        public byte[] ReadPlots(int count)
        {
            return Core.Memory.ReadArray<byte>(Pointer + AgentHousingSelectBlockOffsets.PlotOffset, count);
        }
    }
}