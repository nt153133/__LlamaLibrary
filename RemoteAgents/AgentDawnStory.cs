using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Dawn Story" (Duty Support story mode) interface.
    /// Manages the selection and availability of duties in the story mode Duty Support window.
    /// </summary>
    public class AgentDawnStory : AgentInterface<AgentDawnStory>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentDawnStoryOffsets.Vtable;

        /// <summary>
        /// Gets a value indicating whether the duty list has been fully loaded into memory.
        /// </summary>
        public bool IsLoaded => Core.Memory.Read<byte>(Pointer + AgentDawnStoryOffsets.Loaded) != 0;

        /// <summary>
        /// Gets the memory pointer to the start of the duty information array.
        /// </summary>
        public IntPtr DutyArrayPtr
        {
            get
            {
                var temp1 = Core.Memory.Read<IntPtr>(AgentDawnStoryOffsets.DutyListPtr);

                return temp1 + AgentDawnStoryOffsets.DutyListStart;
            }
        }

        /// <summary>
        /// Gets an array of <see cref="DutyInfo"/> structures representing the available story duties.
        /// </summary>
        public DutyInfo[] Duties => Core.Memory.ReadArray<DutyInfo>(DutyArrayPtr, AgentDawnStoryOffsets.DutyCount);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDawnStory"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentDawnStory(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the identifier of the currently selected duty in the interface.
        /// </summary>
        public byte SelectedDuty => Core.Memory.Read<byte>(Pointer + 0x28);
    }
}