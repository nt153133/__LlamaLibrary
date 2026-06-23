using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Dawn" (Trust / Duty Support) system interface.
    /// Manages selection and state of NPCs for NPC-assisted duties.
    /// </summary>
    public class AgentDawn : AgentInterface<AgentDawn>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentDawnOffsets.DawnVtable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDawn"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentDawn(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets or sets the ID of the currently selected Trust/Duty Support NPC or content row.
        /// </summary>
        public int TrustId
        {
            get => Core.Memory.Read<byte>(Pointer + AgentDawnOffsets.DawnTrustId);
            set => Core.Memory.Write(Pointer + AgentDawnOffsets.DawnTrustId, (byte)value);
        }

        /*
        public bool IsScenario
        {
            get => Core.Memory.Read<byte>(Pointer + AgentDawnOffsets.DawnIsScenario) == 0;
            set => Core.Memory.Write(Pointer + AgentDawnOffsets.DawnIsScenario, value ? (byte)0 : (byte)1);
        }*/
    }
}