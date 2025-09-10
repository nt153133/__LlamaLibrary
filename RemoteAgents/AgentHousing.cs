using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHousing : AgentInterface<AgentHousing>, IAgent
    {
        public IntPtr RegisteredVtable => AgentHousingOffsets.VTable;

        

        protected AgentHousing(IntPtr pointer) : base(pointer)
        {
        }
    }
}