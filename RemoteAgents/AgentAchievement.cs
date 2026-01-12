using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAchievement : AgentInterface<AgentAchievement>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentAchievement;
        

        protected AgentAchievement(IntPtr pointer) : base(pointer)
        {
        }

        public byte Status => Core.Memory.NoCacheRead<byte>(Pointer + AgentAchievementOffsets.Status);
    }
}