using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers;

public static class Timers
{
    private static readonly LLogger Log = new("TimersHelper", Colors.Peru);

    private static readonly FrameCachedValue<ulong> CurrentTimeCachedValue = new(() => Core.Memory.CallInjectedWraper<ulong>(Offsets.GetCurrentTime, 0));

    // ReSharper disable once MemberCanBePrivate.Global
    internal static class Offsets
    {
        [Offset("Search 48 83 EC ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 48 83 C4 ? E9 ? ? ? ? E8 ? ? ? ?")]
        internal static IntPtr GetCurrentTime;

        [Offset("Search E8 ? ? ? ? 48 85 C0 0F 84 ? ? ? ? 41 0F B7 14 5C TraceCall")]
        [OffsetDawntrail("Search 48 83 EC 28 48 8B 05 ? ? ? ? 44 8B C1 BA 1e 01 00 00 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 05 48 83 C4 28")]
        internal static IntPtr GetCycleExd;
    }

    private const int MaxRows = 6;
    private static readonly string[] Description = { "", "Duty/Beast Tribe Dailies", "Weekly Reset", "Unknown", "GC/Rowena", "Unknown" };

    private static readonly CycleTime[] Cycles = new CycleTime[MaxRows];

    public static DateTimeOffset CurrentTime => DateTimeOffset.FromUnixTimeSeconds((long)CurrentTimeStamp).LocalDateTime;

    static Timers()
    {
        for (var i = 0; i < MaxRows; i++)
        {
            Cycles[i] = GetCycleRow(i);
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

    public static ulong CurrentTimeStamp => CurrentTimeCachedValue.Value;

    public static long GetNextCycle(int index)
    {
        var row = Cycles[index];
        return row.FirstCycle + (row.Cycle * ((uint)(ushort)(((uint)CurrentTimeStamp - row.FirstCycle) / row.Cycle) + 1));
    }

    public static CycleTime GetCycleRow(int index)
    {
        var cyclePtr = Core.Memory.CallInjectedWraper<IntPtr>(Offsets.GetCycleExd, index);

        return cyclePtr != IntPtr.Zero ? Core.Memory.Read<CycleTime>(cyclePtr) : default;
    }
}