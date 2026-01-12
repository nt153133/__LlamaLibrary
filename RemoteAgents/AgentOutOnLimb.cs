using System;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has stupid hardcoded memory offsets in Refresh()
    public class AgentOutOnLimb : AgentInterface<AgentOutOnLimb>, IAgent
    {
        public IntPtr RegisteredVtable => PublicOffsets.LLamaAgentIds.AgentOutOnLimb;
        

        public IntPtr addressLocation = IntPtr.Zero;
        private readonly Random rnd = new();

        protected AgentOutOnLimb(IntPtr pointer) : base(pointer)
        {
        }

        public int DoubleDownRemaining => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.DoubleDownRemaining);

        public bool CursorLocked
        {
            get => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.CursorLocked) != 1;
            set => Core.Memory.Write(Pointer + AgentOutOnLimbOffsets.CursorLocked, (byte)(value ? 0 : 1));
        }

        public int CursorLocation
        {
            get => Core.Memory.Read<ushort>(addressLocation);
            set => Core.Memory.Write(addressLocation, LocationValue(value));
        }

        public bool IsReadyBotanist => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 3;

        public bool IsReadyAimg => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 2;

        public void Refresh()
        {
            var intptr_0 = Core.Memory.Read<IntPtr>(Offsets.AtkStage);
            var intptr_1 = Core.Memory.Read<IntPtr>(intptr_0 + 0x38);
            var intptr_2 = Core.Memory.Read<IntPtr>(intptr_1 + 0x18);
            var intptr_3 = Core.Memory.Read<IntPtr>(intptr_2 + AgentOutOnLimbOffsets.LastOffset); //0x310
            addressLocation = Core.Memory.Read<IntPtr>(intptr_3 + AgentOutOnLimbOffsets.LastLastOffset);
        }

        private ushort LocationValue(int percent)
        {
            var location = (ushort)((percent * 100) + rnd.Next(0, 99));

            //Logger.Info($"Setting Location {location}");
            return location;
        }
    }
}