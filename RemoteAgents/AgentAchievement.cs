using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Achievement interface.
    /// Manages the display and state of the player's achievements.
    /// </summary>
    public class AgentAchievement : AgentInterface<AgentAchievement>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentAchievementOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAchievement"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentAchievement(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the current status byte of the achievement agent.
        /// </summary>
        public byte Status => Core.Memory.NoCacheRead<byte>(Pointer + AgentAchievementOffsets.Status);

        private static readonly Dictionary<string, int> VFunctionIds = new()
        {
            { "Show", 3 },
            { "Hide", 5 },
            { "IsAgentActive", 6 },
            { "GetAddonId", 9 },
        };

        /// <summary>
        /// Gets a value indicating whether the agent is currently active by calling its internal "IsAgentActive" virtual function.
        /// </summary>
        private bool IsAgentActive => Core.Memory.CallInjected64<byte>(VFunctionAddress("IsAgentActive"), Pointer) == 1;

        /// <summary>
        /// Gets a value indicating whether the achievement window is currently open.
        /// </summary>
        public bool IsOpen => IsAgentActive;

        private IntPtr VFunctionAddress(string function)
        {
            var index = VFunctionIds[function];

            return Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer) + 0x8 * index);
        }


    }
}