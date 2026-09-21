using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Temporary item shop, prices and inventory capacity.</summary>
    public sealed class XBMContentsItemShop : RemoteWindow<XBMContentsItemShop>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMContentsItemShop() : base("XBMContentsItemShop")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Requests a shop purchase; the caller validates its confirmation.</summary>
        /// <param name="row">Zero-based shop offer row.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool Purchase(uint row)
        {
            if (row >= 30)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (!IsOpen)
            {
                return false;
            }

            SendAction(2, 3, 2, 3, row);
            return true;
        }

        /// <summary>Exits the temporary item shop.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool Exit()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 0);
            return true;
        }

        /// <summary>Detached temporary item shop, prices and inventory capacity data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 269, "XBMContentsItemShop");
                Tokens = Text(data, 1);
                RowCount = data[2].UInt;
                if (RowCount > 30)
                {
                    throw new InvalidOperationException("Invalid shop row count.");
                }
                Offers = Enumerable.Range(0, (int)RowCount).Select(i => new ShopOffer
                {
                    Id = data[4 + 5 * i].UInt,
                    Visible = data[3 + 5 * i].Bool,
                    Unavailable = data[7 + 5 * i].Bool,
                    Price = Text(data, 5 + 5 * i),
                }).ToArray();
                ItemIds = Enumerable.Range(0, 10).Select(i => data[157 + 5 * i].UInt).ToArray();
                GearIds = Enumerable.Range(0, 10).Select(i => data[208 + 5 * i].UInt).ToArray();
            }

            /// <summary>Formatted currency balance.</summary>
            public string Tokens { get; internal set; } = string.Empty;

            /// <summary>Number of populated shop offers.</summary>
            public uint RowCount { get; internal set; }

            /// <summary>Populated offer rows.</summary>
            public ShopOffer[] Offers { get; internal set; } = Array.Empty<ShopOffer>();

            /// <summary>Ten temporary item slots; zero means empty.</summary>
            public uint[] ItemIds { get; internal set; } = Array.Empty<uint>();

            /// <summary>Ten temporary gear slots; zero means empty.</summary>
            public uint[] GearIds { get; internal set; } = Array.Empty<uint>();
        }

        /// <summary>A detached shop offer.</summary>
        public sealed class ShopOffer
        {
            internal ShopOffer()
            {
            }

            /// <summary>Temporary item identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Whether the offer is shown.</summary>
            public bool Visible { get; internal set; }

            /// <summary>Whether the native unavailable flag is set.</summary>
            public bool Unavailable { get; internal set; }

            /// <summary>Formatted price, preserving any native text payloads.</summary>
            public string Price { get; internal set; } = string.Empty;
        }
    }
}