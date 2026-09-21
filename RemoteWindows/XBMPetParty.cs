using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Familiar roster, battlehorn selection, feeding and rest.</summary>
    public sealed class XBMPetParty : RemoteWindow<XBMPetParty>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMPetParty() : base("XBMPetParty")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Toggles the row for horns, feeding or rest in the current mode.</summary>
        /// <param name="row">Zero-based familiar row.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool ToggleRow(uint row)
        {
            if (row >= 15)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (!IsOpen)
            {
                return false;
            }

            SendAction(2, 3, 1, 3, row);
            return true;
        }

        /// <summary>Opens the familiar row context menu.</summary>
        /// <param name="row">Zero-based familiar row.</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool OpenRowContext(uint row)
        {
            if (row >= 15)
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

        /// <summary>Opens the Bestiary for team editing.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool OpenNotebook()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 5);
            return true;
        }

        /// <summary>Confirms feeding or rest selection.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool ConfirmSelection()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 3);
            return true;
        }

        /// <summary>Detached familiar roster, battlehorn selection, feeding and rest data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 1188, "XBMPetParty");
                Mode = data[2].UInt;
                RowCount = data[5].UInt;
                if (RowCount > 15)
                {
                    throw new InvalidOperationException("Invalid familiar row count.");
                }
                Capacity = data[1162].UInt;
                PrimaryButton = Text(data, 1166);
                FeedItem = data[1187].UInt;
                Pets = Enumerable.Range(0, (int)RowCount).Select(i => new PetEntry
                {
                    Row = i,
                    Id = data[82 + 77 * i].UInt,
                    Name = Text(data, 9 + 77 * i),
                    Hp = data[11 + 77 * i].UInt,
                    MaxHp = data[12 + 77 * i].UInt,
                    Horn = data[80 + 77 * i].UInt,
                    RestSelected = data[81 + 77 * i].Bool,
                }).ToArray();
            }

            /// <summary>Zero entry, one overview, two battle setup, three feeding, four rest.</summary>
            public uint Mode { get; internal set; }

            /// <summary>Live familiar count; removed rows beyond this count are stale.</summary>
            public uint RowCount { get; internal set; }

            /// <summary>Maximum entry roster size.</summary>
            public uint Capacity { get; internal set; }

            /// <summary>Temporary item selected for feeding.</summary>
            public uint FeedItem { get; internal set; }

            /// <summary>Localized confirmation button.</summary>
            public string PrimaryButton { get; internal set; } = string.Empty;

            /// <summary>Only the populated familiar rows.</summary>
            public PetEntry[] Pets { get; internal set; } = Array.Empty<PetEntry>();
        }

        /// <summary>A detached pet entry.</summary>
        public sealed class PetEntry
        {
            internal PetEntry()
            {
            }

            /// <summary>Zero-based UI row.</summary>
            public int Row { get; internal set; }

            /// <summary>Familiar species identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Localized familiar name.</summary>
            public string Name { get; internal set; } = string.Empty;

            /// <summary>Current HP.</summary>
            public uint Hp { get; internal set; }

            /// <summary>Maximum HP.</summary>
            public uint MaxHp { get; internal set; }

            /// <summary>Battlehorn slot; three means unassigned.</summary>
            public uint Horn { get; internal set; }

            /// <summary>Camp-rest checkbox; independent of the battlehorn slot.</summary>
            public bool RestSelected { get; internal set; }
        }
    }
}