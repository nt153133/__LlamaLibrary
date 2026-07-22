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
    /// Remote agent for the World Travel Selection interface.
    /// Manages cross-world travel interactions, tracking home and target world choices.
    /// </summary>
    public class AgentWorldTravelSelect : AgentInterface<AgentWorldTravelSelect>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentWorldTravelSelectOffsets.VTable;

#if RB_TC
        /// <summary>The index offset applied to the total world count for the Tencent (China) client.</summary>
        const int MaxCountOffset = -1;
        /// <summary>The number of initial world elements to skip in the choices array for the Tencent (China) client.</summary>
        const int MaxSkip = 0;
#else
        /// <summary>The index offset applied to the total world count.</summary>
        const int MaxCountOffset = -1;
        /// <summary>The number of initial world elements to skip in the choices array to filter out the home world.</summary>
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
        /// Gets the ID of the current world the player is on.
        /// </summary>
        public ushort CurrentWorld => Core.Memory.NoCacheRead<ushort>(ChoicesPointer + 0x4);

        /// <summary>
        /// Gets the memory address pointing to the list of world choices.
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
        /// Gets the list of available world choices for cross-world travel, skipping the home world if applicable based on region.
        /// </summary>
        public WorldChoice[] Choices => Core.Memory.ReadArray<WorldChoice>(ChoicesPointer + 0x8, NumberOfWorlds).Skip(MaxSkip).ToArray();
    }

#if RB_TC
    /// <summary>
    /// Represents a world choice entry in the world travel selection list for the Tencent (China) client.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WorldChoice
    {
        /// <summary>The unique world identifier.</summary>
        public ushort WorldID;

        /// <summary>Unknown or reserved field.</summary>
        public ushort Unk;
    }
#else
    /// <summary>
    /// Represents a 12-byte world choice entry in the world travel selection list.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct WorldChoice
    {
        /// <summary>The unique world identifier.</summary>
        [FieldOffset(0x4)]
        public ushort WorldID;

        /// <summary>Gets the strongly-typed <see cref="World"/> enum representation of the world identifier.</summary>
        public World World => (World)WorldID;
    }
#endif
}
