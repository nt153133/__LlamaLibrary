using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentDawnStory : AgentInterface<AgentDawnStory>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentDawnStory;

        

        public bool IsLoaded => Core.Memory.Read<byte>(Pointer + AgentDawnStoryOffsets.Loaded) != 0;

        public IntPtr DutyArrayPtr
        {
            get
            {
                var temp1 = Core.Memory.Read<IntPtr>(AgentDawnStoryOffsets.DutyListPtr);

                return temp1 + AgentDawnStoryOffsets.DutyListStart;
            }
        }

        public DutyInfo[] Duties => Core.Memory.ReadArray<DutyInfo>(DutyArrayPtr, AgentDawnStoryOffsets.DutyCount);

        protected AgentDawnStory(IntPtr pointer) : base(pointer)
        {
        }

        public byte SelectedDuty => Core.Memory.Read<byte>(Pointer + 0x28);
    }
}