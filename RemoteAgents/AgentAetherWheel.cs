using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAetherWheel : AgentInterface<AgentAetherWheel>, IAgent
    {
        public IntPtr RegisteredVtable => AgentAetherWheelOffsets.VTable;
        

        protected AgentAetherWheel(IntPtr pointer) : base(pointer)
        {
        }

        public AetherWheelSlot[] GetWheelSlots()
        {
            var count = 6;

            if (AetherialWheel.Instance.IsOpen)
            {
                count = AetherialWheel.Instance.MaxSlots;
            }

            using (Core.Memory.TemporaryCacheState(enabledTemporarily: false))
            {
                return Core.Memory.ReadArray<AetherWheelSlot>(Pointer + AgentAetherWheelOffsets.ArrayOffset, count);
            }
        }
    }
}