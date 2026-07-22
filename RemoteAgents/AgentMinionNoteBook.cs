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
    /// Manages the player's minion inventory, providing details on acquired minions and retrieving their names.
    /// </summary>
    public class AgentMinionNoteBook : AgentInterface<AgentMinionNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMinionNoteBookOffsets.VTable;

        /// <summary>
        /// Gets the pointer address where the acquired minion list is located.
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
        /// Retrieves the array of acquired minions currently stored in the player's notebook.
        /// </summary>
        /// <returns>An array of <see cref="MinionStruct"/> representing each acquired minion.</returns>
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
        /// <param name="index">The database index of the minion companion.</param>
        /// <returns>The localized name of the minion, or an empty string if not found.</returns>
        public static string GetMinionName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(AgentMinionNoteBookOffsets.GetCompanion, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x30, Encoding.UTF8) : "";
        }
    }

    /// <summary>
    /// Represents a 4-byte memory mapping for an acquired minion within the player's minion guide.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x4)]
    public struct MinionStruct
    {
        /// <summary>
        /// The database ID of the minion, located at offset 0.
        /// </summary>
        [FieldOffset(0)]
        public ushort MinionId;

        /// <summary>
        /// An unknown field at offset 2, reserved for potential internal state or flags.
        /// </summary>
        [FieldOffset(2)]
        public ushort unknown;
    }
}