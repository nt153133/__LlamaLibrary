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
    /// <summary>
    /// Provides access to the FFXIV achievement system, allowing queries for achievement progress and completion status.
    /// </summary>
    public static class Achievements
    {
        

        /// <summary>Gets the pointer to the game's achievement manager instance.</summary>
        public static IntPtr AchievementInstancePtr => AchievementsOffsets.AchievementInstancePtr;

        /// <summary>Gets the status of the most recently requested single achievement, read directly from game memory.</summary>
        public static AchievementStatus SingleAchievement => Core.Memory.Read<AchievementStatus>(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState);

        /// <summary>
        /// Gets or sets the loading state of the single-achievement request slot.
        /// Set to <see cref="AchievementState.Invalid"/> before issuing a new request to reset the slot.
        /// </summary>
        public static AchievementState SingleAchievementState
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + AchievementsOffsets.SingleAchievementState, (int)value);
        }

        /// <summary>Gets or sets the global achievement manager state.</summary>
        public static AchievementState State
        {
            get => (AchievementState)Core.Memory.Read<int>(AchievementInstancePtr + AchievementsOffsets.AchievementState);
            set => Core.Memory.Write(AchievementInstancePtr + AchievementsOffsets.AchievementState, (int)value);
        }

        /// <summary>
        /// Sends a request to the game to load data for a specific achievement from the server.
        /// </summary>
        /// <param name="achievementId">The numeric ID of the achievement to request.</param>
        public static void RequestAchievement(int achievementId)
        {
            Core.Memory.CallInjectedWraper<byte>(AchievementsOffsets.RequestAchievementFunction, AchievementsOffsets.AchievementInstancePtr, achievementId);
        }

        /// <summary>
        /// Checks synchronously (via injected game function) whether an achievement has been completed.
        /// </summary>
        /// <param name="achievementId">The numeric ID of the achievement to check.</param>
        /// <returns><see langword="true"/> if the achievement is completed; otherwise <see langword="false"/>.</returns>
        /// <example>
        /// <code>
        /// if (Achievements.HasAchievement(achId))
        /// {
        ///     Log.Information("Achievement is already unlocked.");
        /// }
        /// </code>
        /// </example>
        public static bool HasAchievement(int achievementId)
        {
            return Core.Memory.CallInjectedWraper<bool>(AchievementsOffsets.IsCompletePtr, AchievementsOffsets.AchievementInstancePtr, achievementId);
        }

        /// <summary>
        /// Asynchronously fetches the progress status of a single achievement from the server.
        /// </summary>
        /// <param name="achievementId">The numeric ID of the achievement to fetch.</param>
        /// <returns>
        /// An <see cref="AchievementStatus"/> containing the current and maximum progress values,
        /// or default if the request times out.
        /// </returns>
        public static async Task<AchievementStatus> GetSingleAchievement(int achievementId)
        {
            SingleAchievementState = AchievementState.Invalid;

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Invalid);

            RequestAchievement(achievementId);

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Loaded);

            return SingleAchievement;
        }

        /// <summary>
        /// Asynchronously determines whether a specific achievement is fully completed.
        /// </summary>
        /// <param name="achievementId">The numeric ID of the achievement to check.</param>
        /// <returns><see langword="true"/> if the achievement is completed; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> IsCompleteSingleAchievement(int achievementId)
        {
            SingleAchievementState = AchievementState.Invalid;

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Invalid);

            RequestAchievement(achievementId);

            await Coroutine.Wait(5000, () => SingleAchievementState == AchievementState.Loaded);

            return SingleAchievement.IsComplete;
        }
    }

    /// <summary>Represents the loading state of an achievement data request.</summary>
    public enum AchievementState
    {
        /// <summary>Default state; the achievement slot has been reset and is ready for a new request.</summary>
        Invalid = 0,
        /// <summary>A request has been sent to the server but the response has not yet been received.</summary>
        Requested = 1,
        /// <summary>Achievement data has been received from the server and is ready to be read.</summary>
        Loaded = 2,
    }

    /// <summary>
    /// Holds the raw data for a single achievement returned from the game server,
    /// including its ID, current progress, and maximum (completion) progress.
    /// </summary>
    public readonly struct AchievementStatus
    {
        /// <summary>The loading state of this achievement snapshot.</summary>
        public readonly AchievementState State;
        /// <summary>The numeric ID of the achievement.</summary>
        public readonly uint Id;
        /// <summary>The player's current progress toward completing this achievement.</summary>
        public readonly uint CurrentProgress;
        /// <summary>The progress value required for the achievement to be considered complete.</summary>
        public readonly uint MaxProgress;

        /// <summary>Gets a value indicating whether the achievement has been fully completed.</summary>
        public bool IsComplete => CurrentProgress == MaxProgress;

        /// <summary>Gets a value indicating whether the achievement data has been loaded from the server.</summary>
        public bool IsLoaded => State == AchievementState.Loaded;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Id: {Id}, Progress: {CurrentProgress} / {MaxProgress}, IsComplete: {IsComplete}";
        }
    }
}