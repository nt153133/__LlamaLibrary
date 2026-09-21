using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Board overview and battle-entry details.</summary>
    public sealed class XBMStageDetailList : RemoteWindow<XBMStageDetailList>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMStageDetailList() : base("XBMStageDetailList")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(Elements);

        /// <summary>Challenges the board or starts its displayed battle.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool ActivatePrimary()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 8);
            return true;
        }

        /// <summary>Closes the displayed board overview.</summary>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool CloseOverview()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(1, 3, 0);
            return true;
        }

        /// <summary>Detached board overview and battle-entry details data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                RequireLength(data, 40058, "XBMStageDetailList");
                Mode = data[1].UInt;
                RowCount = data[5].UInt;
                if (RowCount > 1000)
                {
                    throw new InvalidOperationException("Invalid board row count.");
                }
                // The fixed tail follows the allocation for 1,000 stride-40 rows.
                Difficulty = data[40049].UInt;
                PrimaryButton = Text(data, 40056);
                EventId = data[7].UInt;
                Events = Enumerable.Range(0, (int)RowCount).Select(i => new EventEntry
                {
                    Id = data[7 + 40 * i].UInt,
                    Move = data[8 + 40 * i].UInt,
                    Completion = data[12 + 40 * i].UInt,
                    Name = Text(data, 13 + 40 * i),
                }).ToArray();
            }

            /// <summary>Zero for entry, one for overview, two for battle setup.</summary>
            public uint Mode { get; internal set; }

            /// <summary>Number of populated event/enemy rows.</summary>
            public uint RowCount { get; internal set; }

            /// <summary>Selected difficulty; zero is Standard.</summary>
            public uint Difficulty { get; internal set; }

            /// <summary>First displayed event identifier.</summary>
            public uint EventId { get; internal set; }

            /// <summary>Localized primary button label.</summary>
            public string PrimaryButton { get; internal set; } = string.Empty;

            /// <summary>Displayed events, including expanded enemy rows.</summary>
            public EventEntry[] Events { get; internal set; } = Array.Empty<EventEntry>();
        }

        /// <summary>A detached event entry.</summary>
        public sealed class EventEntry
        {
            internal EventEntry()
            {
            }

            /// <summary>Event identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Board move number.</summary>
            public uint Move { get; internal set; }

            /// <summary>Zero pending, one completed, two skipped.</summary>
            public uint Completion { get; internal set; }

            /// <summary>Localized row description.</summary>
            public string Name { get; internal set; } = string.Empty;
        }
    }
}