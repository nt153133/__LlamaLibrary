using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Character" window.
    /// Provides access to character attributes, equipment, and profile data.
    /// </summary>
    public class AgentCharacter : AgentInterface<AgentCharacter>, IAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentCharacter"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentCharacter(IntPtr pointer) : base(pointer)
        {
        }

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentCharacterOffsets.Vtable;
    }
}