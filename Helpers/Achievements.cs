using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
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

            [Offset("Search C7 81 ? ? ? ? ? ? ? ? 45 33 C9 B9 ? ? ? ? Add 2 Read32")]
            internal static int AchievementState;

            [Offset("Search 48 83 EC ?? C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 45 33 C9")]
            internal static IntPtr RequestAchievementFunction;

            [Offset("Search 44 89 81 ? ? ? ? 44 89 89 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 8B 81 ? ? ? ? Add 3 Read32")]
            internal static int AchievementCurrentProgress;

            [Offset("Search C7 81 ? ? ? ? ? ? ? ? 45 33 C9 B9 ? ? ? ? Add 2 Read32")]
            internal static int SingleAchievementState;
        }

        public static IntPtr AchievementInstancePtr => Offsets.AchievementInstancePtr;

        public static AchievementStatus SingleAchievement => Core.Memory.Read<AchievementStatus>(AchievementInstancePtr + Offsets.SingleAchievementState);

        public static AchievementState SingleAchievementState
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + Offsets.SingleAchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + Offsets.SingleAchievementState, (int)value);
        }

        public static AchievementState State
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + Offsets.AchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + Offsets.AchievementState, (int)value);
        }

        public static void RequestAchievement(int achievementId)
        {
            Core.Memory.CallInjected64<byte>(Offsets.RequestAchievementFunction, Offsets.AchievementInstancePtr, achievementId);
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

        public static async Task<AchievementStatus> GetSingleAchievement(int achievementId)
        {
            SingleAchievementState = AchievementState.Invalid;

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Invalid);

            RequestAchievement(achievementId);

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Loaded);

            return SingleAchievement;
        }

        public static async Task<bool> IsCompleteSingleAchievement(int achievementId)
        {
            SingleAchievementState = AchievementState.Invalid;

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Invalid);

            RequestAchievement(achievementId);

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Loaded);

            return SingleAchievement.IsComplete;
        }
    }

    public enum AchievementState
    {
        Invalid = 0, // Achievement is initialized at this state
        Requested = 1, // This state is set between the client request and receiving the data from the server
        Loaded = 2, // Set upon data being received
    }

    public readonly struct AchievementStatus
    {
        public readonly AchievementState State;
        public readonly uint Id;
        public readonly uint CurrentProgress;
        public readonly uint MaxProgress;

        public bool IsComplete => CurrentProgress == MaxProgress;

        public bool IsLoaded => State == AchievementState.Loaded;

        public override string ToString()
        {
            return $"Id: {Id}, Progress: {CurrentProgress} / {MaxProgress}, IsComplete: {IsComplete}";
        }
    }
}