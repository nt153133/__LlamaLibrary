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
    /// Remote agent for the "World Travel" (World Visit System) interface.
    /// Manages the selection of destination worlds and retrieves available world choices.
    /// </summary>
    /// <remarks>
    /// Under standard client builds, the home world is filtered out of the selectable destination world choices.
    /// This is controlled via the <c>MaxSkip</c> constant using conditional compilation.
    /// </remarks>
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentWorldTravelSelectOffsets.VTable;

#if RB_TC
        /// <summary>
        /// The offset added to the raw world count to obtain the actual number of destination choices.
        /// </summary>
        const int MaxCountOffset = -1;

        /// <summary>
        /// The number of elements to skip from the beginning of the world choices list.
        /// Under <c>RB_TC</c>, this is 0.
        /// </summary>
        const int MaxSkip = 0;
#else
        /// <summary>
        /// The offset added to the raw world count to obtain the actual number of destination choices.
        /// </summary>
        const int MaxCountOffset = -1;

        /// <summary>
        /// The number of elements to skip from the beginning of the world choices list.
        /// Under standard clients, this is 1 to skip/filter the player's home world from selectable destinations.
        /// </summary>
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
        /// Gets the ID of the currently selected world in the world travel interface.
        /// </summary>
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);

        /// <summary>
        /// Gets the pointer to the world choices list in game memory.
        /// </summary>
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentWorldTravelSelectOffsets.ChoicesOffset);

        /// <summary>
        /// Gets the ID of the player's home world.
        /// </summary>
        public ushort HomeWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer);

        /// <summary>
        /// Gets the total number of worlds available in the travel selection list.
        /// </summary>
        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + AgentWorldTravelSelectOffsets.MaxWorldOffset) + MaxCountOffset;

        /// <summary>
        /// Gets the array of destination world choices available for travel.
        /// </summary>
        /// <value>
        /// An array of <see cref="WorldChoice"/> structures representing the destination worlds.
        /// </value>
        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds).Skip(MaxSkip).ToArray();
    }

#if RB_TC
    /// <summary>
    /// Represents a world choice option in the world travel interface list for TC builds.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        /// <summary>
        /// Gets the unique identifier for the world.
        /// </summary>
        public ushort WorldID;

        /// <summary>
        /// Gets an unknown/unused field value.
        /// </summary>
        public ushort Unk;
    }
#else
    /// <summary>
    /// Represents a world choice option in the world travel interface list.
    /// Maps a 0xC-byte memory entry representing a world choice option to the world details.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct WorldChoice
    {
        /// <summary>
        /// Gets the unique identifier for the world, located at offset 0x4.
        /// </summary>
        [FieldOffset(0x4)]
        public ushort WorldID;

        /// <summary>
        /// Gets the corresponding <see cref="World"/> enum value for this world choice.
        /// </summary>
        public World World => (World)WorldID;
    }
#endif
}