using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFateProgress : AgentInterface<AgentFateProgress>, IAgent
    {
        public IntPtr RegisteredVtable => AgentFateProgressOffsets.VTable;
        

        public int NumberOfLoadedZones => 0; //Core.Memory.NoCacheRead<byte>(Pointer + Offsets.LoadedZones);

        public SharedFateProgress[] ProgressArray => new SharedFateProgress[0];

        //Core.Memory.ReadArray<SharedFateProgress>(Core.Memory.Read<IntPtr>(Pointer + Offsets.ZoneStructs), NumberOfLoadedZones);

        protected AgentFateProgress(IntPtr pointer) : base(pointer)
        {
        }
    }
}