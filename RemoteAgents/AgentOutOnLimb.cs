using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the "Out on a Limb" and "Finer Miner" (Logging and Mining) mini-games.
    /// Manages the game state, including cursor position, double-down availability, and readiness status.
    /// </summary>
    public class AgentOutOnLimb : AgentInterface<AgentOutOnLimb>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentOutOnLimbOffsets.VTable;

        /// <summary>
        /// The memory address where the current cursor location value is stored.
        /// </summary>
        public IntPtr addressLocation = IntPtr.Zero;

        private readonly Random rnd = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentOutOnLimb"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentOutOnLimb(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the number of double-down opportunities remaining in the current session.
        /// </summary>
        public int DoubleDownRemaining => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.DoubleDownRemaining);

        /// <summary>
        /// Gets or sets a value indicating whether the mini-game cursor is currently locked.
        /// </summary>
        public bool CursorLocked
        {
            get => Core.Memory.Read<byte>(Pointer + AgentOutOnLimbOffsets.CursorLocked) != 1;
            set => Core.Memory.Write(Pointer + AgentOutOnLimbOffsets.CursorLocked, (byte)(value ? 0 : 1));
        }

        /// <summary>
        /// Gets or sets the current horizontal position of the mini-game cursor.
        /// </summary>
        [Obsolete("Use Director Instead")]
        public int CursorLocation
        {
            get => Core.Memory.Read<ushort>(addressLocation);
            set => Core.Memory.Write(addressLocation, LocationValue(value));
        }

        /// <summary>
        /// Gets a value indicating whether the Botanist (Logging) mini-game is ready to play.
        /// </summary>
        public bool IsReadyBotanist => Core.Memory.NoCacheRead<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 3;

        /// <summary>
        /// Gets a value indicating whether the Miner (Aming) mini-game is ready to play.
        /// </summary>
        public bool IsReadyAimg => Core.Memory.NoCacheRead<byte>(Pointer + AgentOutOnLimbOffsets.IsReady) == 2;

        /// <summary>
        /// Refreshes the <see cref="addressLocation"/> by resolving it from the game's number array data.
        /// Must be called when the mini-game starts or the UI updates.
        /// </summary>
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