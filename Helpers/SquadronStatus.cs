using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public static class SquadronStatus
    {
        

        public static SquadronTimerData RawStruct => Core.Memory.Read<SquadronTimerData>(SquadronStatusOffsets.SquadronStatus);

        private static readonly TimeSpan CachePeriod = new(0, 1, 0);

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