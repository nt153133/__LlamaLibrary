using System;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the World Travel selection interface.
    /// Manages cross-world visit options and retrieves available target worlds.
    /// </summary>
    /// <remarks>
    /// In other regions, a skip logic (<c>MaxSkip = 1</c>) is used to filter out the player's home world from the target choices,
    /// while for the Tencent version (RB_TC), no home world skipping is applied (<c>MaxSkip = 0</c>).
    /// </remarks>
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentWorldTravelSelectOffsets.VTable;

#if RB_TC
        const int MaxCountOffset = -1;
        const int MaxSkip = 0;
#else
        const int MaxCountOffset = -1;
        const int MaxSkip = 1;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentWorldTravelSelect"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentWorldTravelSelect(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the ID of the current world the player is physically on.
        /// </summary>
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);

        /// <summary>
        /// Gets the pointer to the world choices data structure array.
        /// </summary>
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentWorldTravelSelectOffsets.ChoicesOffset);

        /// <summary>
        /// Gets the ID of the player's Home World.
        /// </summary>
        public ushort HomeWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer);

        /// <summary>
        /// Gets the total number of worlds in the current logical data center/choices list.
        /// </summary>
        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + AgentWorldTravelSelectOffsets.MaxWorldOffset) + MaxCountOffset;

        /// <summary>
        /// Gets the collection of available worlds for cross-world travel.
        /// </summary>
        /// <remarks>
        /// Filters out the Home World in standard configurations using <c>MaxSkip</c>.
        /// </remarks>
        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds).Skip(MaxSkip).ToArray();
    }

#if RB_TC
    /// <summary>
    /// Represents a world travel destination choice for the Tencent game client.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        /// <summary>
        /// The unique ID representing the destination game world.
        /// </summary>
        public ushort WorldID;

        /// <summary>
        /// An unknown field alignment/metadata value.
        /// </summary>
        public ushort Unk;
    }
#else
    /// <summary>
    /// Represents a world travel destination choice.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct WorldChoice
    {
        /// <summary>
        /// The unique ID representing the destination game world.
        /// </summary>
        [FieldOffset(0x4)]
        public ushort WorldID;

        /// <summary>
        /// Gets the corresponding strongly-typed <see cref="World"/> enum representation for the world ID.
        /// </summary>
        public World World => (World)WorldID;
    }
#endif
}