using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has hardcoded memory offsets and i'm not actually sure why it's here
    public class AgentRetainerCharacter : AgentInterface<AgentRetainerCharacter>, IAgent
    {
        public IntPtr RegisteredVtable => AgentRetainerCharacterOffsets.VTable;
        

        protected AgentRetainerCharacter(IntPtr pointer) : base(pointer)
        {
        }

        public int ILvl => Core.Memory.Read<byte>(Pointer + 0xa78);
    }
}