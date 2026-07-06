using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for Gold Saucer information.
    /// Provides access to data and state related to the Manderville Gold Saucer, such as currency and mini-game status.
    /// </summary>
    public class AgentGoldSaucerInfo : AgentInterface<AgentGoldSaucerInfo>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentGoldSaucerInfoOffsets.VTable;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentGoldSaucerInfo"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentGoldSaucerInfo(IntPtr pointer) : base(pointer)
        {
        }
    }
}
