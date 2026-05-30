using System;
using System.Runtime.InteropServices;

namespace LlamaLibrary.RetainerItemFinder
{
    /// <summary>
    /// Represents a node in the binary tree used by the game's internal item finder system.
    /// This structure is mapped directly from game memory.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public struct ItemFinderPtrNode
    {
        /// <summary>
        /// Gets the pointer to the left child node in the binary tree.
        /// </summary>
        [FieldOffset(0x0)]
        public readonly IntPtr Left;

        /// <summary>
        /// Gets the pointer to the parent node in the binary tree.
        /// </summary>
        [FieldOffset(0x8)]
        public readonly IntPtr Parent;

        /// <summary>
        /// Gets the pointer to the right child node in the binary tree.
        /// </summary>
        [FieldOffset(0x10)]
        public readonly IntPtr Right;

        /// <summary>
        /// Gets the status byte indicating whether this node is filled with valid data.
        /// A value of 0 typically means the node is valid/filled.
        /// </summary>
        [FieldOffset(0x19)]
        public readonly byte FilledStatus;

        /// <summary>
        /// Gets the unique identifier (Content ID) of the retainer associated with this node.
        /// </summary>
        [FieldOffset(0x20)]
        public readonly ulong RetainerId;

        /// <summary>
        /// Gets the pointer to the raw inventory data for the retainer.
        /// </summary>
        [FieldOffset(0x28)]
        public readonly IntPtr RetainerInventory;

        /// <summary>
        /// Gets a value indicating whether this node contains valid data.
        /// </summary>
        public bool Filled => FilledStatus == 0;
    }
}
