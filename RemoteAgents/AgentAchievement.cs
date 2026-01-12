using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAchievement : AgentInterface<AgentAchievement>, IAgent
    {
        public IntPtr RegisteredVtable => AgentAchievementOffsets.VTable;

        protected AgentAchievement(IntPtr pointer) : base(pointer)
        {
        }

        public byte Status => Core.Memory.NoCacheRead<byte>(Pointer + AgentAchievementOffsets.Status);

        private static readonly Dictionary<string, int> VFunctionIds = new()
        {
            { "Show", 3 },
            { "Hide", 5 },
            { "IsAgentActive", 6 },
            { "GetAddonId", 9 },
        };

        private bool IsAgentActive => Core.Memory.CallInjected64<byte>(VFunctionAddress("IsAgentActive"), Pointer) == 1;

        public bool IsOpen => IsAgentActive;

        private IntPtr VFunctionAddress(string function)
        {
            var index = VFunctionIds[function];

            return Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer) + 0x8 * index);
        }


    }
}