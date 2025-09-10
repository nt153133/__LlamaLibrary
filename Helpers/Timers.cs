using System;
using System.Windows.Media;
using ff14bot;
using ff14bot.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers;

public static class Timers
{
    private static readonly LLogger Log = new("TimersHelper", Colors.Peru);

    private static readonly FrameCachedValue<ulong> CurrentTimeCachedValue = new(() => Core.Memory.CallInjectedWraper<ulong>(TimersOffsets.GetCurrentTime, 0));

    // ReSharper disable once MemberCanBePrivate.Global
    

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
        var cyclePtr = Core.Memory.CallInjectedWraper<IntPtr>(TimersOffsets.GetCycleExd, index);

        return cyclePtr != IntPtr.Zero ? Core.Memory.Read<CycleTime>(cyclePtr) : default;
    }
}