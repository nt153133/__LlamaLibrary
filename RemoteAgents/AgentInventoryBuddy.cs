using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentInventoryBuddy : AgentInterface<AgentInventoryBuddy>, IAgent
    {
        public IntPtr RegisteredVtable => AgentInventoryBuddyOffsets.VTable;

        

        protected AgentInventoryBuddy(IntPtr pointer) : base(pointer)
        {
        }
    }
}