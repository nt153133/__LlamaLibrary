using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHandIn : AgentInterface<AgentHandIn>, IAgent
    {
        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentHandIn;
        

        protected AgentHandIn(IntPtr pointer) : base(pointer)
        {
        }

        public void HandIn(BagSlot slot)
        {
            Core.Memory.CallInjectedWraper<uint>(
                                                 Offsets.HandInFunc,
                                                 Pointer + AgentHandInOffsets.HandinParmOffset,
                                                 slot.Slot,
                                                 (int)slot.BagId);
        }
    }
}