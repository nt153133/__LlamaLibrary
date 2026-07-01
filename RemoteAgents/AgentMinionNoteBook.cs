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
    /// Provides access to the list of acquired minions and their metadata.
    /// </summary>
    public class AgentMinionNoteBook : AgentInterface<AgentMinionNoteBook>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMinionNoteBookOffsets.VTable;

        /// <summary>
        /// Gets the memory address where the pointer to the minion list is stored.
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
        /// Retrieves the full list of minions currently loaded in the agent's memory.
        /// </summary>
        /// <returns>An array of <see cref="MinionStruct"/> objects.</returns>
        public MinionStruct[] GetCurrentMinions()
        {
            var address = Pointer + AgentMinionNoteBookOffsets.AgentOffset;
            var address1 = Core.Memory.Read<IntPtr>(address);
            var count = Core.Memory.Read<uint>(AgentMinionNoteBookOffsets.MinionCount);
            return Core.Memory.ReadArray<MinionStruct>(address1, (int)count);
        }

        /// <summary>
        /// Retrieves the localized name of a minion by its index in the guide.
        /// </summary>
        /// <param name="index">The zero-based index of the minion.</param>
        /// <returns>The minion name as a string, or an empty string if not found.</returns>
        public static string GetMinionName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(AgentMinionNoteBookOffsets.GetCompanion, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x30, Encoding.UTF8) : "";
        }
    }

    /// <summary>
    /// Represents the memory-mapped structure for a minion in the guide.
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
        /// Reserved or unknown field.
        /// </summary>
        [FieldOffset(2)]
        public ushort unknown;
    }
}