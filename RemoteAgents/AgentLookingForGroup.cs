using System;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentLookingForGroup : AgentInterface<AgentLookingForGroup>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ?? ?? ?? ?? 48 8B F1 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 41 ?? E8 ?? ?? ?? ?? Add 3 TraceRelative")]
            internal static IntPtr VTable;

            //0x2240
            [Offset("Search 48 8D 8B ? ? ? ? 41 B8 ? ? ? ? 48 8B F8 Add 3 Read32")]
            internal static int CommentString;
        }

        protected AgentLookingForGroup(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr CommentStringPtr => Pointer + Offsets.CommentString;

        public string SavedComment
        {
            get => Core.Memory.ReadStringUTF8(CommentStringPtr);
            set => Core.Memory.WriteString(CommentStringPtr, value, Encoding.UTF8);
        }
    }
}