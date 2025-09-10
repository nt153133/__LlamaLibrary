using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentBagSlot : AgentInterface<AgentBagSlot>, IAgent
    {
        public IntPtr RegisteredVtable => AgentBagSlotOffsets.VTable;
        

        protected AgentBagSlot(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr PointerForAether => Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(Pointer + AgentBagSlotOffsets.Offset) + (0x20 * 7) + AgentBagSlotOffsets.FuncOffset);
    }
}