using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentRecommendEquip : AgentInterface<AgentRecommendEquip>, IAgent
    {
        public IntPtr RegisteredVtable => AgentRecommendEquipOffsets.VTable;
        

        protected AgentRecommendEquip(IntPtr pointer) : base(pointer)
        {
        }
    }
}