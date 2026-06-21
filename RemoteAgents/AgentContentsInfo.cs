using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Contents Info" (Duty Recorder / Duty List) interface.
    /// </summary>
    public class AgentContentsInfo : AgentInterface<AgentContentsInfo>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentContentsInfoOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentContentsInfo"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentContentsInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}