using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Shared FATE" progress interface.
    /// Manages information about rank and completion status for FATEs across different zones.
    /// </summary>
    public class AgentFateProgress : AgentInterface<AgentFateProgress>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentFateProgressOffsets.VTable;

        /// <summary>
        /// Gets the number of zones with loaded FATE progress data.
        /// </summary>
        public int NumberOfLoadedZones => 0; //Core.Memory.NoCacheRead<byte>(Pointer + Offsets.LoadedZones);

        /// <summary>
        /// Gets an array of <see cref="SharedFateProgress"/> structures for the loaded zones.
        /// </summary>
        public SharedFateProgress[] ProgressArray => new SharedFateProgress[0];

        //Core.Memory.ReadArray<SharedFateProgress>(Core.Memory.Read<IntPtr>(Pointer + Offsets.ZoneStructs), NumberOfLoadedZones);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentFateProgress"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentFateProgress(IntPtr pointer) : base(pointer)
        {
        }
    }
}