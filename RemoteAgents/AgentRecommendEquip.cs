using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Recommended Gear" interface.
    /// Handles the automated selection and display of optimal equipment for the current class or job.
    /// </summary>
    public class AgentRecommendEquip : AgentInterface<AgentRecommendEquip>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentRecommendEquipOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRecommendEquip"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentRecommendEquip(IntPtr pointer) : base(pointer)
        {
        }
    }
}