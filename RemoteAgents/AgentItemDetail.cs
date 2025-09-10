using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemDetail : AgentInterface<AgentItemDetail>, IAgent
    {
        

        protected AgentItemDetail(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => AgentItemDetailOffsets.Vtable;

        public uint HoverOverItemID => Core.Memory.Read<uint>(Pointer + AgentItemDetailOffsets.ItemID);
    }
}