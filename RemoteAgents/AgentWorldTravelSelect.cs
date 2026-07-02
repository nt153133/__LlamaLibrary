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
    /// Facilitates moving between worlds within the same Data Center.
    /// </summary>
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
        /// Gets the ID of the currently selected world in the interface.
        /// </summary>
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);

        /// <summary>
        /// Gets the base memory pointer for the list of available world choices.
        /// </summary>
        public IntPtr ChoicesPointer => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentWorldTravelSelectOffsets.ChoicesOffset);

        /// <summary>
        /// Gets the ID of the player's home world.
        /// </summary>
        public ushort HomeWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer);

        /// <summary>
        /// Gets the total number of worlds available for selection.
        /// </summary>
        public int NumberOfWorlds => Core.Memory.NoCacheRead<byte>(Pointer + AgentWorldTravelSelectOffsets.MaxWorldOffset) + MaxCountOffset;

        /// <summary>
        /// Gets the array of available worlds to travel to, filtered by region-specific rules.
        /// </summary>
        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds).Skip(MaxSkip).ToArray();
    }

#if RB_TC
    /// <summary>
    /// Represents a world travel option in the selection list (Traditional Chinese region).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        /// <summary>The internal world identifier.</summary>
        public ushort WorldID;
        /// <summary>Unknown regional data.</summary>
        public ushort Unk;
    }
#else
    /// <summary>
    /// Represents a world travel option in the selection list.
    /// Maps to a 0xC byte structure in memory.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct WorldChoice
    {
        /// <summary>The internal world identifier.</summary>
        [FieldOffset(0x4)]
        public ushort WorldID;

        /// <summary>Gets the <see cref="World"/> enum value associated with <see cref="WorldID"/>.</summary>
        public World World => (World)WorldID;
    }
#endif
}