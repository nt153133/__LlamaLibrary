using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Gold Saucer information tracking system.
    /// Manages client-side state and data queries relating to Gold Saucer currencies, stats, and mini-games.
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