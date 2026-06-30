using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the retainer's character and attributes interface.
    /// Provides access to retainer-specific stats such as item level.
    /// </summary>
    //TODO This agent has hardcoded memory offsets and i'm not actually sure why it's here
    public class AgentRetainerCharacter : AgentInterface<AgentRetainerCharacter>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentRetainerCharacterOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRetainerCharacter"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentRetainerCharacter(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the current average item level of the retainer's equipped gear.
        /// </summary>
        public int ILvl => Core.Memory.Read<byte>(Pointer + 0xa78);
    }
}