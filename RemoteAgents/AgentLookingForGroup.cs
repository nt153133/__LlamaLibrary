using System;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Party Finder (Looking for Group) interface.
    /// Manages recruitment comments and other PF-related settings.
    /// </summary>
    public class AgentLookingForGroup : AgentInterface<AgentLookingForGroup>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentLookingForGroupOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentLookingForGroup"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentLookingForGroup(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the memory address where the Party Finder recruitment comment is stored.
        /// </summary>
        public IntPtr CommentStringPtr => Pointer + AgentLookingForGroupOffsets.CommentString;

        /// <summary>
        /// Gets or sets the Party Finder recruitment comment string.
        /// </summary>
        public string SavedComment
        {
            get => Core.Memory.ReadStringUTF8(CommentStringPtr);
            set => Core.Memory.WriteString(CommentStringPtr, value, Encoding.UTF8);
        }
    }
}