using System;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentLookingForGroup : AgentInterface<AgentLookingForGroup>, IAgent
    {
        public IntPtr RegisteredVtable => AgentLookingForGroupOffsets.VTable;

        

        protected AgentLookingForGroup(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr CommentStringPtr => Pointer + AgentLookingForGroupOffsets.CommentString;

        public string SavedComment
        {
            get => Core.Memory.ReadStringUTF8(CommentStringPtr);
            set => Core.Memory.WriteString(CommentStringPtr, value, Encoding.UTF8);
        }
    }
}