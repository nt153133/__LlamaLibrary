using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the FC Aetherial Wheel interface.
    /// Manages information about the aetherial wheel stand, including slot statuses and priming progress.
    /// </summary>
    public class AgentAetherWheel : AgentInterface<AgentAetherWheel>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentAetherWheelOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAetherWheel"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentAetherWheel(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Retrieves the state of all slots in the Aetherial Wheel stand from game memory.
        /// </summary>
        /// <returns>An array of <see cref="AetherWheelSlot"/> structures representing each slot's current status.</returns>
        public AetherWheelSlot[] GetWheelSlots()
        {
            var count = 6;

            if (AetherialWheel.Instance.IsOpen)
            {
                count = AetherialWheel.Instance.MaxSlots;
            }

            return Core.Memory.ReadArray<AetherWheelSlot>(Pointer + AgentAetherWheelOffsets.ArrayOffset, count);
        }
    }
}