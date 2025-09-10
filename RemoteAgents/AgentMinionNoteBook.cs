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
    public class AgentMinionNoteBook : AgentInterface<AgentMinionNoteBook>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMinionNoteBookOffsets.VTable;

        public IntPtr MinionListAddress => Pointer + AgentMinionNoteBookOffsets.AgentOffset;
        

        protected AgentMinionNoteBook(IntPtr pointer) : base(pointer)
        {
        }

        public MinionStruct[] GetCurrentMinions()
        {
            var address = Pointer + AgentMinionNoteBookOffsets.AgentOffset;
            var address1 = Core.Memory.Read<IntPtr>(address);
            var count = Core.Memory.Read<uint>(AgentMinionNoteBookOffsets.MinionCount);
            return Core.Memory.ReadArray<MinionStruct>(address1, (int)count);
        }

        public static string GetMinionName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(AgentMinionNoteBookOffsets.GetCompanion, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x30, Encoding.UTF8) : "";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x4)]
    public struct MinionStruct
    {
        [FieldOffset(0)]
        public ushort MinionId;

        [FieldOffset(2)]
        public ushort unknown;
    }
}