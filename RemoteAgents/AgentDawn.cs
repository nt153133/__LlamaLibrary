using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentDawn : AgentInterface<AgentDawn>, IAgent
    {
        public IntPtr RegisteredVtable => AgentDawnOffsets.DawnVtable;
        

        protected AgentDawn(IntPtr pointer) : base(pointer)
        {
        }

        public int TrustId
        {
            get => Core.Memory.Read<byte>(Pointer + AgentDawnOffsets.DawnTrustId);
            set => Core.Memory.Write(Pointer + AgentDawnOffsets.DawnTrustId, (byte)value);
        }

        /*
        public bool IsScenario
        {
            get => Core.Memory.Read<byte>(Pointer + AgentDawnOffsets.DawnIsScenario) == 0;
            set => Core.Memory.Write(Pointer + AgentDawnOffsets.DawnIsScenario, value ? (byte)0 : (byte)1);
        }*/
    }
}