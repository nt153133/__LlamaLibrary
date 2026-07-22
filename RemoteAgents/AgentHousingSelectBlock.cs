using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing ward selection interface.
    /// Manages the housing ward selection UI, tracking selected ward numbers and plot status data.
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
        /// Gets or sets the selected ward number index.
        /// </summary>
        /// <value>The zero-based ward index in the housing area selection.</value>
        public int WardNumber
        {
            get => Core.Memory.Read<int>(Pointer + AgentHousingSelectBlockOffsets.WardNumber);
            set => Core.Memory.Write(Pointer + AgentHousingSelectBlockOffsets.WardNumber, value);
        }

        /// <summary>
        /// Reads the status of housing plots for the selected ward block.
        /// </summary>
        /// <param name="count">The number of plots to read status for (usually up to 30 or 60 depending on ward size/subdivision).</param>
        /// <returns>A byte array representing plot availability/purchase status in the current ward block.</returns>
        public byte[] ReadPlots(int count)
        {
            return Core.Memory.ReadArray<byte>(Pointer + AgentHousingSelectBlockOffsets.PlotOffset, count);
        }
    }
}