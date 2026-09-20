using System;
using System.Linq;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Board selection list.</summary>
    public sealed class XBMStageList : CrucibleWindow<XBMStageList>
    {
        /// <summary>Creates a wrapper without opening the window.</summary>
        public XBMStageList() : base("XBMStageList")
        {
        }

        /// <summary>Copies the visible window's data on the bot thread or within a frame lock.</summary>
        /// <returns>A detached snapshot; IsOpen is false when the window is closed.</returns>
        public Snapshot ReadSnapshot() => new Snapshot(ReadVisibleValues());

        /// <summary>Selects a board by its native identifier.</summary>
        /// <param name="board">Board identifier (1..5).</param>
        /// <returns>Whether a callback was submitted; verify the resulting UI state.</returns>
        public bool SelectBoard(uint board)
        {
            if (board < 1 || board > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(board));
            }

            return Callback(3, 1, 3, board);
        }

        /// <summary>Detached board selection list data.</summary>
        public sealed class Snapshot : WindowSnapshot
        {
            internal Snapshot(TwoInt[] data) : base(data.Length)
            {
                if (!IsOpen)
                {
                    return;
                }

                if (data.Length < 2 || data[1].UInt > 5 || data.Length < 2 + data[1].UInt * 2)
                {
                    throw new InvalidOperationException("Unrecognized board list.");
                }
                RowCount = data[1].UInt;
                Boards = Enumerable.Range(0, (int)RowCount).Select(i => new BoardEntry
                {
                    Id = data[3 + 2 * i].UInt,
                    Name = Text(data, 2 + 2 * i),
                }).ToArray();
            }

            /// <summary>Number of listed boards.</summary>
            public uint RowCount { get; internal set; }

            /// <summary>Available board IDs and localized names.</summary>
            public BoardEntry[] Boards { get; internal set; } = Array.Empty<BoardEntry>();
        }

        /// <summary>A detached board entry.</summary>
        public sealed class BoardEntry
        {
            internal BoardEntry()
            {
            }

            /// <summary>Native board identifier.</summary>
            public uint Id { get; internal set; }

            /// <summary>Localized board name.</summary>
            public string Name { get; internal set; } = string.Empty;
        }
    }
}