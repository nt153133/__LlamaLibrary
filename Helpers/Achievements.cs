using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    public static class Achievements
    {
        private static class Offsets
        {
            [Offset("Search E8 ?? ?? ?? ?? 04 30 TraceCall")]
            [OffsetDawntrail("Search E8 ?? ?? ?? ?? 04 30 FF C3 TraceCall")]
            internal static IntPtr IsCompletePtr;

            [Offset("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 04 30 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 04 30 FF C3 Add 3 TraceRelative")]
            internal static IntPtr AchievementInstancePtr;
        }

        public static bool HasAchievement(int achievementId)
        {
            bool done;

            lock (Core.Memory.Executor.AssemblyLock)
            {
                done = Core.Memory.CallInjected64<bool>(Offsets.IsCompletePtr, Offsets.AchievementInstancePtr, achievementId);
            }

            return done;
        }
    }
}