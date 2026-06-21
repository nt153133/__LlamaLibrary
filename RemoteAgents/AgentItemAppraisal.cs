using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the item appraisal interface (e.g., for Eureka/Bozja lockboxes or PotD/HoH sacks).
    /// </summary>
    public class AgentItemAppraisal : AgentInterface<AgentItemAppraisal>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentItemAppraisalOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentItemAppraisal"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentItemAppraisal(IntPtr pointer) : base(pointer)
        {
        }
    }
}