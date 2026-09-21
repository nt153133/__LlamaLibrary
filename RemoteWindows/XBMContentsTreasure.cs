using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Treasure selection and already-owned temporary gear.</summary>
    public sealed class XBMContentsTreasure : RemoteWindow<XBMContentsTreasure>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMContentsTreasure() : base("XBMContentsTreasure")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Chooses an offered treasure row.</summary>
        /// <param name="row">Zero-based offer row (0..3).</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool Choose(uint row)
        {
            if (row >= 4)
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

        /// <summary>Detached treasure selection and already-owned temporary gear data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 144, "XBMContentsTreasure");
                Offers = Enumerable.Range(0, 4).Select(i => new TreasureOffer
                {
                    Id = data[6 + 5 * i].UInt,
                    Visible = data[3 + 5 * i].Bool,
                    // Offered rows remain visible after selection; acquisition is separate.
                    Acquired = data[126 + i].Bool,
                }).ToArray();
                Gear = Enumerable.Range(0, 10).Select(i => new GearSlot
                {
                    Id = data[78 + 5 * i].UInt,
                    Visible = data[75 + 5 * i].Bool,
                }).ToArray();
            }

            /// <summary>Four offered rows, including hidden or acquired choices.</summary>
            public TreasureOffer[] Offers { get; internal set; } = Array.Empty<TreasureOffer>();

            /// <summary>Ten temporary gear slots.</summary>
            public GearSlot[] Gear { get; internal set; } = Array.Empty<GearSlot>();
        }

        /// <summary>A detached treasure offer.</summary>
        public sealed class TreasureOffer
        {
            internal TreasureOffer()
            {
            }

            /// <summary>Temporary item identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Whether the row is displayed.</summary>
            public bool Visible { get; internal set; }

            /// <summary>Whether this coffer's choice has already been acquired.</summary>
            public bool Acquired { get; internal set; }
        }

        /// <summary>A detached gear slot.</summary>
        public sealed class GearSlot
        {
            internal GearSlot()
            {
            }

            /// <summary>Temporary gear identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Whether the occupied slot is displayed.</summary>
            public bool Visible { get; internal set; }
        }
    }
}