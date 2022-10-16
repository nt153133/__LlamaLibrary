using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentDawnStory : AgentInterface<AgentDawnStory>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.Vtable;

        private static class Offsets
        {
            [Offset("48 8D 05 ? ? ? ? C6 43 ? ? 48 89 03 48 8B C3 48 C7 43 ? ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            [Offset("48 89 05 ? ? ? ? 48 83 C4 ? C3 48 C7 05 ? ? ? ? ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 3 TraceRelative")]
            internal static IntPtr DutyListPtr;

            [Offset("48 8D 99 ? ? ? ? 48 8D 4C 24 ? Add 3 Read32")]
            [OffsetCN("48 8D 93 ? ? ? ? E8 ? ? ? ? 48 8B 8C 24 ? ? ? ? Add 3 Read32")]//changes in 6.2
            internal static int DutyListStart;

            [Offset("BF ? ? ? ? 48 8B D3 48 8B CE Add 1 Read32")]
            internal static int DutyCount;

            //8B 5F ? C7 47 ? ? ? ? ? 48 8B 01 FF 50 ? 8B D3 48 8B C8 4C 8B 00 41 FF 90 ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 83 79 ? ?

            [Offset("8B 5F ? C7 47 ? ? ? ? ? 48 8B 01 FF 50 ? 8B D3 48 8B C8 4C 8B 00 41 FF 90 ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 83 79 ? ? Add 2 Read8")]
            internal static int Loaded;
        }

        public bool IsLoaded => Core.Memory.Read<byte>(Pointer + Offsets.Loaded) != 0;

        public IntPtr DutyArrayPtr
        {
            get
            {
                var temp1 = Core.Memory.Read<IntPtr>(Offsets.DutyListPtr);

                return temp1 + Offsets.DutyListStart;
            }
        }

        public DutyInfo[] Duties => Core.Memory.ReadArray<DutyInfo>(DutyArrayPtr, Offsets.DutyCount);

        protected AgentDawnStory(IntPtr pointer) : base(pointer)
        {
        }

        public byte SelectedDuty => Core.Memory.Read<byte>(Pointer + 0x28);
    }
}