using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentHandIn : AgentInterface<AgentHandIn>, IAgent
    {
        public IntPtr RegisteredVtable => AgentHandInOffsets.VTable;
        

        protected AgentHandIn(IntPtr pointer) : base(pointer)
        {
        }

        public void HandIn(BagSlot slot)
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjectedWraper<uint>(
                                                     Offsets.HandInFunc,
                                                     Pointer + AgentHandInOffsets.HandinParmOffset,
                                                     slot.Slot,
                                                     (int)slot.BagId);
            }
        }
    }
}