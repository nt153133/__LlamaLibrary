using System;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;


namespace LlamaLibrary.Helpers
{
#if RB_DT
    /// <summary>
    /// Reads the active duty director's objective (todo) argument list from game memory.
    /// The todo list reflects the current duty's objective checklist shown in the UI.
    /// Only available in RebornBuddy DT (Dawntrail) builds.
    /// </summary>
    public class DirectorHelper
    {

        private static IntPtr CurrentDirector;
        private static IntPtr CurrentDirectorTodoArgs;

        /// <summary>Gets the memory address of the currently active duty director, or <see cref="IntPtr.Zero"/> if none is active.</summary>
        public static IntPtr ActiveDirectorAddress => Core.Memory.Read<IntPtr>(DirectorHelperOffsets.ActiveDirector);

        /// <summary>
        /// Reads the active director's todo argument array from game memory.
        /// Each entry corresponds to one objective slot in the duty's objective list.
        /// </summary>
        /// <returns>An array of <see cref="TodoArgs"/>; empty if no director is active.</returns>
        public static TodoArgs[] GetTodoArgs()
        {
            if (ActiveDirectorAddress == IntPtr.Zero)
            {
                return Array.Empty<TodoArgs>();
            }

            if (ActiveDirectorAddress != CurrentDirector)
            {
                CurrentDirector = ActiveDirectorAddress;
                CurrentDirectorTodoArgs = Core.Memory.CallInjectedWraper<IntPtr>(DirectorHelperOffsets.GetTodoArgs, DirectorHelperOffsets.ActiveDirector);
            }

            if (CurrentDirector == IntPtr.Zero)
            {
                return Array.Empty<TodoArgs>();
            }

            var twovec = Core.Memory.Read<NativeVectorV2<TodoArgs>>(CurrentDirectorTodoArgs);
            return Core.Memory.ReadArray<TodoArgs>(twovec.First, twovec.Count);
        }

        /// <summary>
        /// Determines whether the objective at the given index is complete
        /// (i.e., <see cref="TodoArgs.CountCurrent"/> equals <see cref="TodoArgs.CountNeeded"/>).
        /// </summary>
        /// <param name="pos">Zero-based index of the objective to check.</param>
        /// <returns><see langword="true"/> if the objective is checked/complete; otherwise <see langword="false"/>.</returns>
        public static bool IsTodoChecked(uint pos)
        {
            var args = GetTodoArgs();

            if (pos > args.Length) {
                return false;
            }

            var arg = args[pos];

            if (arg.DisplayType == 0)
            {
                return false;
            }

            return arg.CountCurrent == arg.CountNeeded;
        }

        /// <summary>
        /// Memory layout of a single director todo objective entry.
        /// Mirrors the in-game <c>DirectorTodo</c> struct used by the duty director system.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 0x160)]
        public struct TodoArgs
        {
            /// <summary>Whether this objective slot is active/visible.</summary>
            [FieldOffset(0x00)] public bool Enabled;

            /// <summary>Rendering mode for this objective (0 = hidden/inactive).</summary>
            [FieldOffset(0x04)] public int DisplayType;

            /// <summary>Current progress count for this objective.</summary>
            [FieldOffset(0x78)] public int CountCurrent;

            /// <summary>Required count to consider this objective complete.</summary>
            [FieldOffset(0x7C)] public int CountNeeded;

            /// <summary>Remaining time for time-limited objectives (game ticks).</summary>
            [FieldOffset(0x80)] public ulong TimeLeft;

            /// <summary>The <c>Map</c> sheet row ID associated with this objective, if any.</summary>
            [FieldOffset(0x88)] public uint MapRowId;
        }
    }

#endif
}