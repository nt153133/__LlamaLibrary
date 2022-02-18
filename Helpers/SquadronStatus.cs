using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class SquadronStatus
    {
        private static class Offsets
        {
            [Offset("Search 8B 3D ? ? ? ? 8B D8 3B F8 Add 2 TraceRelative")]
            internal static IntPtr SquadronStatus;
        }

        public static SquadronTimerData RawStruct => Core.Memory.Read<SquadronTimerData>(Offsets.SquadronStatus);

        private static readonly TimeSpan CachePeriod = new TimeSpan(0, 1, 0);

        private static DateTime lastCheck;

        private static SquadronTimerData _cachedData;

        public static void Update()
        {
            _cachedData = RawStruct;

            lastCheck = DateTime.Now;
        }

        public static SquadronTimerData Status
        {
            get
            {
                if (DateTime.Now - lastCheck < CachePeriod)
                {
                    return _cachedData;
                }

                Update();

                return _cachedData;
            }
        }

        public static bool MissionDone => Status.MissionEndTime <= DateTime.Now;

        public static bool TrainingDone => Status.TrainingEnd <= DateTime.Now;
    }
}