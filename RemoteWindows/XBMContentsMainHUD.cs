using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Crucible HUD inventory and board-overview control.</summary>
    public sealed class XBMContentsMainHUD : RemoteWindow<XBMContentsMainHUD>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMContentsMainHUD() : base("XBMContentsMainHUD")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Opens the board overview without suspending or forfeiting.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool OpenOverview()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 0);
            return true;
        }

        /// <summary>Opens a populated, enabled temporary item slot's context menu.</summary>
        /// <param name="slot">Zero-based inventory slot (0..9).</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool OpenItemContext(uint slot)
        {
            if (slot >= 10)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            if (!IsOpen)
            {
                return false;
            }

            // Crucible items are director inventory, not normal BagSlots. The
            // HUD publishes their selectable state; names are display data only.
            var values = Elements;
            if (values.Length == 0)
            {
                return false;
            }

            WindowSnapshot.RequireLength(values, 111, "XBMContentsMainHUD");
            int row = (int)slot * 5;
            if (!values[10 + row].Bool || values[12 + row].UInt == 0)
            {
                return false;
            }

            SendAction(2, 3, 6, 3, slot);
            return true;
        }

        /// <summary>Detached crucible hud inventory and board-overview control data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 111, "XBMContentsMainHUD");
                Tokens = Text(data, 8);
                Items = Enumerable.Range(0, 10).Select(i => new ItemSlot
                {
                    Id = data[12 + 5 * i].UInt,
                    Name = Text(data, 13 + 5 * i),
                    Enabled = data[10 + 5 * i].Bool,
                }).ToArray();
            }

            /// <summary>Formatted temporary-currency text.</summary>
            public string Tokens { get; internal set; } = string.Empty;

            /// <summary>Ten temporary item slots, including empty slots.</summary>
            public ItemSlot[] Items { get; internal set; } = Array.Empty<ItemSlot>();
        }

        /// <summary>A detached item slot.</summary>
        public sealed class ItemSlot
        {
            internal ItemSlot()
            {
            }

            /// <summary>Temporary item identifier; zero means empty.</summary>
            public uint Id { get; internal set; }

            /// <summary>Localized item name.</summary>
            public string Name { get; internal set; } = string.Empty;

            /// <summary>Whether the item can currently be selected.</summary>
            public bool Enabled { get; internal set; }
        }
    }
}