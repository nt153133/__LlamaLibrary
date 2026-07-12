using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the housing ward selection interface.
    /// Manages the display and selection of residential wards and their plot availability.
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
        /// Gets or sets the zero-based index of the currently selected ward in the selection list.
        /// </summary>
        public int WardNumber
        {
            get => Core.Memory.Read<int>(Pointer + AgentHousingSelectBlockOffsets.WardNumber);
            set => Core.Memory.Write(Pointer + AgentHousingSelectBlockOffsets.WardNumber, value);
        }

        /// <summary>
        /// Reads the status of housing plots from game memory for the selected ward.
        /// </summary>
        /// <param name="count">The number of plots to read (typically 30 or 60 depending on the ward type).</param>
        /// <returns>A byte array representing plot availability or status.</returns>
        public byte[] ReadPlots(int count)
        {
            return Core.Memory.ReadArray<byte>(Pointer + AgentHousingSelectBlockOffsets.PlotOffset, count);
        }
    }
}
