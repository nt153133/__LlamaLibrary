using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentCharacter : AgentInterface<AgentCharacter>, IAgent
    {
        

        protected AgentCharacter(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => AgentCharacterOffsets.Vtable;
    }
}