using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Helpers;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Remote window and memory-backed data reader for the Challenge Log.</summary>
    public sealed class ContentsNote : RemoteWindow<ContentsNote>
    {
        private const int ContentsNoteOffset = 0x4380;
        private const int CompletionFlagsOffset = 0x08;
        private const int StateOffset = 0x18;
        private const int SelectedTabOffset = 0x1C;
        private const int DisplayCountOffset = 0x1D;
        private const int DisplayIdsOffset = 0x20;
        private const int DisplayStatusesOffset = 0x6C;
        private const int MaximumRows = 104;
        private const int MaximumDisplayedRows = 19;

        public ContentsNote() : base("ContentsNote")
        {
        }

        public int State => Read<int>(StateOffset);
        public byte SelectedTab => Read<byte>(SelectedTabOffset);
        public byte DisplayCount => Read<byte>(DisplayCountOffset);

        public bool IsComplete(int rowId)
        {
            if (rowId < 1 || rowId > MaximumRows || DataPointer == IntPtr.Zero)
                return false;

            var bitIndex = rowId - 1;
            var value = Core.Memory.Read<byte>(IntPtr.Add(
                DataPointer,
                CompletionFlagsOffset + (bitIndex / 8)));
            return (value & (1 << (bitIndex % 8))) != 0;
        }

        public IReadOnlyList<ContentsNoteEntry> DisplayEntries()
        {
            var count = Math.Min(MaximumDisplayedRows, (int)DisplayCount);
            var result = new List<ContentsNoteEntry>(count);
            for (var index = 0; index < count; index++)
            {
                var rowId = Read<int>(DisplayIdsOffset + index * sizeof(int));
                var status = Read<int>(DisplayStatusesOffset + index * sizeof(int));
                if (rowId > 0)
                    result.Add(new ContentsNoteEntry(rowId, status, IsComplete(rowId)));
            }

            return result;
        }

        public override async Task<bool> Open()
        {
            if (State == 2)
                return true;

            if (!IsOpen)
                AgentContentsNote.Instance.Toggle();

            return await Coroutine.Wait(5000, () => State == 2);
        }

        private static IntPtr DataPointer => UIState.Instance == IntPtr.Zero
            ? IntPtr.Zero
            : IntPtr.Add(UIState.Instance, ContentsNoteOffset);

        private static T Read<T>(int offset) where T : unmanaged
            => DataPointer == IntPtr.Zero
                ? default
                : Core.Memory.Read<T>(IntPtr.Add(DataPointer, offset));
    }

    public sealed class ContentsNoteEntry
    {
        public ContentsNoteEntry(int rowId, int rawStatus, bool complete)
        {
            RowId = rowId;
            RawStatus = rawStatus;
            Complete = complete;
        }

        public int RowId { get; }
        public int RawStatus { get; }
        public bool Complete { get; }
    }
}
