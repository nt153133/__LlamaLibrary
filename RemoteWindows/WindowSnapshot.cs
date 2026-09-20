using System;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Copied addon values; native string pointers are decoded before the snapshot leaves the frame.</summary>
    public abstract class WindowSnapshot
    {
        /// <summary>Records whether a visible window supplied values.</summary>
        /// <param name="valueCount">Native value count, or zero when closed.</param>
        protected WindowSnapshot(int valueCount)
        {
            ValueCount = valueCount;
        }

        /// <summary>Whether the snapshot contains an open window's values.</summary>
        public bool IsOpen => ValueCount != 0;

        /// <summary>Native value count for diagnostics; zero when closed.</summary>
        public int ValueCount { get; }

        internal static void RequireLength(TwoInt[] values, int expected, string window)
        {
            if (values.Length != expected)
            {
                throw new InvalidOperationException($"Unrecognized {window} layout: expected {expected} values, received {values.Length}.");
            }
        }

        internal static string Text(TwoInt[] values, int index)
        {
            if (index < 0 || index >= values.Length)
            {
                return string.Empty;
            }

            // Strip the managed-storage flag; String (8) and String8 (10) carry UTF-8 pointers.
            int type = (int)values[index].AtkValueType & 15;
            return type == 8 || type == 10 ? values[index].AsUTF8String : string.Empty;
        }
    }
}