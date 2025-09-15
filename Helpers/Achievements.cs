using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;
// ReSharper disable UnassignedReadonlyField

namespace LlamaLibrary.Helpers
{
    public static class Achievements
    {
        

        public static IntPtr AchievementInstancePtr => AchievementsOffsets.AchievementInstancePtr;

        public static AchievementStatus SingleAchievement => Core.Memory.Read<AchievementStatus>(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState);

        public static AchievementState SingleAchievementState
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState, (int)value);
        }

        public static AchievementState State
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + AchievementsOffsets.AchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + AchievementsOffsets.AchievementState, (int)value);
        }

        public static void RequestAchievement(int achievementId)
        {
            Core.Memory.CallInjectedWraper<byte>(AchievementsOffsets.RequestAchievementFunction, AchievementsOffsets.AchievementInstancePtr, achievementId);
        }

        public static bool HasAchievement(int achievementId)
        {
            bool done;

            lock (Core.Memory.Executor.AssemblyLock)
            {
                done = Core.Memory.CallInjectedWraper<bool>(AchievementsOffsets.IsCompletePtr, AchievementsOffsets.AchievementInstancePtr, achievementId);
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