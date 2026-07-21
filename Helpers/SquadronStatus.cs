using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides access to the player's Grand Company Squadron status, including mission and training completion timers.
    /// Data is read from game memory and cached for 1 minute to minimize memory access.
    /// </summary>
    public static class SquadronStatus
    {
        /// <summary>
        /// Reads the raw <see cref="SquadronTimerData"/> structure directly from game memory.
        /// </summary>
        public static SquadronTimerData RawStruct => Core.Memory.Read<SquadronTimerData>(SquadronStatusOffsets.SquadronStatus);

        /// <summary>
        /// The duration for which the squadron status data is considered valid before requiring a refresh.
        /// </summary>
        private static readonly TimeSpan CachePeriod = new(0, 1, 0);

        /// <summary>
        /// The timestamp of the last successful memory read.
        /// </summary>
        private static DateTime lastCheck;

        /// <summary>
        /// The locally cached copy of the squadron timer data.
        /// </summary>
        private static SquadronTimerData _cachedData;

        /// <summary>
        /// Forces an immediate refresh of the cached squadron status data by reading from game memory.
        /// </summary>
        public static void Update()
        {
            _cachedData = RawStruct;

            lastCheck = DateTime.Now;
        }

        /// <summary>
        /// Gets the current squadron timer data. Returns cached data if within the <see cref="CachePeriod"/>,
        /// otherwise refreshes the cache from memory.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the current squadron mission has been completed.
        /// </summary>
        public static bool MissionDone => Status.MissionEndTime <= DateTime.Now;

        /// <summary>
        /// Gets a value indicating whether the current squadron training session has been completed.
        /// </summary>
        public static bool TrainingDone => Status.TrainingEnd <= DateTime.Now;

        /// <summary>Gets whether an autonomous Squadron mission is currently underway.</summary>
        public static bool HasActiveMission
            => Status.MissionId != 0 && Status.MissionEndTime > DateTime.Now;

        /// <summary>Gets whether a dispatched Squadron mission has returned and awaits handling.</summary>
        public static bool HasReturnedMission
            => Status.MissionId != 0 && Status.MissionEndTime <= DateTime.Now;

        /// <summary>Gets the remaining autonomous mission time, clamped to zero.</summary>
        public static TimeSpan MissionRemaining
            => HasActiveMission ? Status.MissionEndTime - DateTime.Now : TimeSpan.Zero;

        /// <summary>Gets whether Squadron training is currently underway.</summary>
        public static bool HasActiveTraining
            => Status.TrainingId != 0 && Status.TrainingEnd > DateTime.Now;

        /// <summary>Gets whether Squadron training has finished and awaits handling.</summary>
        public static bool HasReturnedTraining
            => Status.TrainingId != 0 && Status.TrainingEnd <= DateTime.Now;

        /// <summary>Gets the remaining training time, clamped to zero.</summary>
        public static TimeSpan TrainingRemaining
            => HasActiveTraining ? Status.TrainingEnd - DateTime.Now : TimeSpan.Zero;
    }
}
