using System;
using System.Runtime.InteropServices;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Minion Guide (Minion Notebook) interface.
    /// Manages the player's collection of acquired minions, allowing lookups of unlocked minion IDs and names.
    /// </summary>
    public class AgentMinionNoteBook : AgentInterface<AgentMinionNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMinionNoteBookOffsets.VTable;

        /// <summary>
        /// Gets the memory address pointing to the pointer of the acquired minion list array.
        /// </summary>
        public IntPtr MinionListAddress => Pointer + AgentMinionNoteBookOffsets.AgentOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMinionNoteBook"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMinionNoteBook(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Retrieves the array of acquired minions from game memory.
        /// </summary>
        /// <returns>An array of <see cref="MinionStruct"/> structures containing the IDs of all unlocked minions.</returns>
        public MinionStruct[] GetCurrentMinions()
        {
            var address = Pointer + AgentMinionNoteBookOffsets.AgentOffset;
            var address1 = Core.Memory.Read<IntPtr>(address);
            var count = Core.Memory.Read<uint>(AgentMinionNoteBookOffsets.MinionCount);
            return Core.Memory.ReadArray<MinionStruct>(address1, (int)count);
        }

        /// <summary>
        /// Gets the name of the minion at the specified database or row index by calling the game's internal function.
        /// </summary>
        /// <param name="index">The index or row ID of the minion companion.</param>
        /// <returns>The localized name of the minion, or an empty string if not found.</returns>
        public static string GetMinionName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(AgentMinionNoteBookOffsets.GetCompanion, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x30, Encoding.UTF8) : "";
        }
    }

    /// <summary>
    /// Represents a 4-byte memory mapping for an acquired minion record.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x4)]
    public struct MinionStruct
    {
        /// <summary>
        /// The unique identifier of the minion.
        /// </summary>
        [FieldOffset(0)]
        public ushort MinionId;

        /// <summary>
        /// Reserved or unknown field at offset 2 of the structure.
        /// </summary>
        [FieldOffset(2)]
        public ushort unknown;
    }
}
