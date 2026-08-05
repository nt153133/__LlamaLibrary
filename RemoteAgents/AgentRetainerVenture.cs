using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the retainer venture interface.
    /// Provides access to venture rewards, experience gain, and the current venture task ID.
    /// </summary>
    public class AgentRetainerVenture : AgentInterface<AgentRetainerVenture>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentRetainerVentureOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRetainerVenture"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentRetainerVenture(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the amount of experience points earned by the retainer from the completed venture.
        /// </summary>
        public int ExperiencedGain => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.ExperienceGain);

        /// <summary>
        /// Gets the ID of the first reward item obtained from the venture.
        /// </summary>
        public int RewardItem1 => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RewardItem1);

        /// <summary>
        /// Gets the ID of the second reward item obtained from the venture, if applicable.
        /// </summary>
        public int RewardItem2 => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RewardItem2);

        /// <summary>
        /// Gets the quantity of the first reward item obtained.
        /// </summary>
        public int RewardCount1 => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RewardCounts);

        /// <summary>
        /// Gets the quantity of the second reward item obtained.
        /// </summary>
        public int RewardCount2 => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RewardCount2);

        /// <summary>
        /// Gets the ID of the venture task currently associated with the retainer or the result window.
        /// </summary>
        public int RetainerTask => Core.Memory.Read<int>(Pointer + AgentRetainerVentureOffsets.RetainerTask);
    }
}
