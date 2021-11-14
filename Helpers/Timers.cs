using System;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class Timers
    {
        private static readonly string Name = "TimersHelper";
        private static readonly Color LogColor = Colors.Peru;
        private static readonly LLogger Log = new LLogger(Name, LogColor);

        internal static class Offsets
        {
            [Offset("Search 48 83 EC ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 48 83 C4 ? E9 ? ? ? ? E8 ? ? ? ?")]
            internal static IntPtr GetCurrentTime;

            [Offset("Search E8 ? ? ? ? 48 85 C0 0F 84 ? ? ? ? 41 0F B7 14 5C TraceCall")]
            internal static IntPtr GetCycleExd;
        }

        private const int MaxRows = 6;
        private static readonly string[] Description = { "", "Duty/Beast Tribe Dailies", "Weekly Reset", "Unknown", "GC/Rowena", "Unknown" };

        private static CycleTime[] _cycles = new CycleTime[MaxRows];

        public static DateTimeOffset CurrentTime => DateTimeOffset.FromUnixTimeSeconds((long)CurrentTimeStamp).LocalDateTime;

        static Timers()
        {
            for (var i = 0; i < MaxRows; i++)
            {
                _cycles[i] = GetCycleRow(i);
            }
        }

        public static void PrintTimers()
        {
            Log.Information($"Current Time: ({CurrentTime.LocalDateTime})");
            for (var i = 1; i < MaxRows; i++)
            {
                var time = DateTimeOffset.FromUnixTimeSeconds(GetNextCycle(i));

                Log.Information($"{time.LocalDateTime} ({Description[i]})");
            }
        }

        public static ulong CurrentTimeStamp
        {
            get
            {
                ulong currentTime;
                lock (Core.Memory.Executor.AssemblyLock)
                {
                    currentTime = Core.Memory.CallInjected64<ulong>(Offsets.GetCurrentTime, 0);
                }

                return currentTime;
            }
        }

        public static long GetNextCycle(int index)
        {
            var row = _cycles[index];
            Log.Information($"Getting Cycle: ({index})");
            return row.FirstCycle + (row.Cycle * ((uint)(ushort)(((uint)CurrentTimeStamp - row.FirstCycle) / row.Cycle) + 1));
        }

        public static CycleTime GetCycleRow(int index)
        {
            IntPtr CyclePtr;
            lock (Core.Memory.Executor.AssemblyLock)
            {
                CyclePtr = Core.Memory.CallInjected64<IntPtr>(Offsets.GetCycleExd, index);
            }

            if (CyclePtr != IntPtr.Zero)
            {
                return Core.Memory.Read<CycleTime>(CyclePtr);
            }

            return default;
        }
    }
}