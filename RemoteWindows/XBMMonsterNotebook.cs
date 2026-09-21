using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Master's Bestiary catalog and team editor.</summary>
    public sealed class XBMMonsterNotebook : RemoteWindow<XBMMonsterNotebook>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMMonsterNotebook() : base("XBMMonsterNotebook")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Changes the displayed catalog page.</summary>
        /// <param name="page">Zero-based catalog page.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool SetCatalogPage(uint page)
        {
            if (page > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            // Catalog browsing used Int64 (4); team editing used UInt (5).
            // Preserve the captured argument types for each mode.
            if (!IsOpen)
            {
                return false;
            }

            SendAction(2, 3, 3, 4, page);
            return true;
        }

        /// <summary>Changes the team-edit page using its observed callback type.</summary>
        /// <param name="page">Zero-based team-edit page.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool SetTeamPage(uint page)
        {
            if (page > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            if (!IsOpen)
            {
                return false;
            }

            SendAction(2, 3, 3, 5, page);
            return true;
        }

        /// <summary>Toggles the displayed familiar in the team.</summary>
        /// <param name="cell">Zero-based cell; verify its species before calling.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool ToggleTeamEntry(uint cell)
        {
            if (cell >= 25)
            {
                throw new ArgumentOutOfRangeException(nameof(cell));
            }

            if (!IsOpen)
            {
                return false;
            }

            SendAction(2, 3, 7, 5, cell);
            return true;
        }

        /// <summary>Detached master's bestiary catalog and team editor data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 284, "XBMMonsterNotebook");
                Beasts = Enumerable.Range(0, 25).Select(i => new BeastEntry
                {
                    Id = data[24 + 8 * i].UInt,
                    Captured = data[26 + 8 * i].Bool,
                }).ToArray();
            }

            /// <summary>The 25 displayed cells; verify species IDs before toggling a cell.</summary>
            public BeastEntry[] Beasts { get; internal set; } = Array.Empty<BeastEntry>();
        }

        /// <summary>A detached beast entry.</summary>
        public sealed class BeastEntry
        {
            internal BeastEntry()
            {
            }

            /// <summary>Familiar species identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Whether this familiar is captured.</summary>
            public bool Captured { get; internal set; }
        }
    }
}