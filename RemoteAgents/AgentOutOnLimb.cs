using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    //TODO This agent has stupid hardcoded memory offsets in Refresh()
    public class AgentOutOnLimb : AgentInterface<AgentOutOnLimb>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 8D 4F ? 48 89 07 E8 ? ? ? ? 33 C9 Add 3 TraceRelative")]
            internal static IntPtr VTable;
            [Offset("Search 41 80 BE ? ? ? ? ? 0F 84 ? ? ? ? BA ? ? ? ? Add 3 Read32")]
            internal static int IsReady;
            [Offset("Search 41 C6 86 ? ? ? ? ? EB ? 41 C6 86 ? ? ? ? ? Add 3 Read32")]
            internal static int CursorLocked;
            [Offset("89 9F ? ? ? ? 48 8B 5C 24 ? 89 B7 ? ? ? ? 48 8B 74 24 ? 89 AF ? ? ? ? Add 2 Read32")]
            internal static int DoubleDownRemaining;
            [Offset("Search 48 8B AA ? ? ? ? 48 8B F9 48 85 ED Add 3 Read32")]
            internal static int LastOffset;
        }

        public IntPtr addressLocation = IntPtr.Zero;
        private Random rnd = new Random();

        protected AgentOutOnLimb(IntPtr pointer) : base(pointer)
        {
        }

        public int DoubleDownRemaining => Core.Memory.Read<byte>(Pointer + Offsets.DoubleDownRemaining);

        public bool CursorLocked
        {
            get => Core.Memory.Read<byte>(Pointer + Offsets.CursorLocked) != 1;
            set => Core.Memory.Write(Pointer + Offsets.CursorLocked, (byte)(value ? 0 : 1));
        }

        public int CursorLocation
        {
            get => Core.Memory.Read<ushort>(addressLocation);
            set => Core.Memory.Write(addressLocation, LocationValue(value));
        }

        public bool IsReadyBotanist => Core.Memory.Read<byte>(Pointer + Offsets.IsReady) == 3;

        public bool IsReadyAimg => Core.Memory.Read<byte>(Pointer + Offsets.IsReady) == 2;

        public void Refresh()
        {
            var intptr_0 = Core.Memory.Read<IntPtr>(Memory.Offsets.AtkStage);
            var intptr_1 = Core.Memory.Read<IntPtr>(intptr_0 + 0x38);
            var intptr_2 = Core.Memory.Read<IntPtr>(intptr_1 + 0x18);
            var intptr_3 = Core.Memory.Read<IntPtr>(intptr_2 + Offsets.LastOffset);//0x310
            addressLocation = Core.Memory.Read<IntPtr>(intptr_3 + 0x20);
        }

        private ushort LocationValue(int percent)
        {
            var location = (ushort)((percent * 100) + rnd.Next(0, 99));

            //Logger.Info($"Setting Location {location}");
            return location;
        }
    }
}