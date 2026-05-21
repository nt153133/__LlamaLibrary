using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentOutOnLimb : AgentInterface<AgentOutOnLimb>, IAgent
    {
        public IntPtr RegisteredVtable => AgentOutOnLimbOffsets.VTable;


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

        [Obsolete("Use Director Instead")]
        public int CursorLocation
        {
            get => Core.Memory.Read<ushort>(addressLocation);
            set => Core.Memory.Write(addressLocation, LocationValue(value));
        }

        public bool IsReadyBotanist => Core.Memory.NoCacheRead<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 3;

        public bool IsReadyAimg => Core.Memory.NoCacheRead<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 2;

        public void Refresh()
        {
            var numArray = AtkArrayDataHolder.NumberArray(104);

            if (numArray == IntPtr.Zero)
            {
                return;
            }

            addressLocation = Core.Memory.Read<IntPtr>(numArray + RetainerHistoryOffsets.NumberArrayData_IntArray);

        }

        private ushort LocationValue(int percent)
        {
            var location = (ushort)((percent * 100) + rnd.Next(0, 99));

            //Logger.Info($"Setting Location {location}");
            return Math.Clamp(location, (ushort)0, (ushort)9999);
        }
    }
}