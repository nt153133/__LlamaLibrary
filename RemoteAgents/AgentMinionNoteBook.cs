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
    /// Remote agent for the Minion Guide (Notebook) interface.
    /// Manages the display and querying of acquired minions and their names.
    /// </summary>
    public class AgentMinionNoteBook : AgentInterface<AgentMinionNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMinionNoteBookOffsets.VTable;

        /// <summary>
        /// Gets the raw memory address of the minion list.
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
        /// Retrieves the list of currently owned or displayed minions from game memory.
        /// </summary>
        /// <returns>An array of <see cref="MinionStruct"/> representing the player's minion notebook list.</returns>
        public MinionStruct[] GetCurrentMinions()
        {
            var address = Pointer + AgentMinionNoteBookOffsets.AgentOffset;
            var address1 = Core.Memory.Read<IntPtr>(address);
            var count = Core.Memory.Read<uint>(AgentMinionNoteBookOffsets.MinionCount);
            return Core.Memory.ReadArray<MinionStruct>(address1, (int)count);
        }

        /// <summary>
        /// Resolves the in-game name of a minion by its database index.
        /// </summary>
        /// <param name="index">The internal database or row index of the minion companion.</param>
        /// <returns>The UTF-8 string name of the minion, or an empty string if not found.</returns>
        public static string GetMinionName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(AgentMinionNoteBookOffsets.GetCompanion, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x30, Encoding.UTF8) : "";
        }
    }

    /// <summary>
    /// Represents a 4-byte memory mapping for a minion entry in the player's collection.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x4)]
    public struct MinionStruct
    {
        /// <summary>
        /// The unique ID of the minion companion.
        /// </summary>
        [FieldOffset(0)]
        public ushort MinionId;

        /// <summary>
        /// An unknown/alignment field.
        /// </summary>
        [FieldOffset(2)]
        public ushort unknown;
    }
}