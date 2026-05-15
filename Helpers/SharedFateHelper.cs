using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides cached access to the player's Shared FATE progress across Shadowbringers zones.
    /// Shared FATE is a zone-wide reputation system where players earn Bicolor Gemstones
    /// by completing FATEs in specific areas. Progress is cached for 1 minute to reduce UI open/close overhead.
    /// </summary>
    public static class SharedFateHelper
    {
        private static DateTime lastWindowCheck;

        /// <summary>Most recently fetched Shared FATE progress array, cached for up to 1 minute.</summary>
        public static SharedFateProgress[] CachedProgress;

        private static readonly TimeSpan CachePeriod = new(0, 1, 0);

        static SharedFateHelper()
        {
            lastWindowCheck = new DateTime(1970, 1, 1);
        }

        /// <summary>
        /// Returns the Shared FATE progress for the specified zone ID, refreshing the cache if needed.
        /// </summary>
        /// <param name="zoneId">Territory/zone ID to look up.</param>
        /// <returns>The matching <see cref="SharedFateProgress"/>, or default if not found.</returns>
        public static async Task<SharedFateProgress> GetSharedFateProgress(uint zoneId)
        {
            return (await CachedRead()).FirstOrDefault(i => i.Zone == zoneId);
        }

        /// <summary>
        /// Returns the cached FATE progress array if fresh; otherwise opens the FATE Progress window,
        /// reads all zone progress, closes the window, and updates the cache.
        /// </summary>
        /// <returns>An array of all <see cref="SharedFateProgress"/> entries.</returns>
        public static async Task<SharedFateProgress[]> CachedRead()
        {
            if (DateTime.Now - lastWindowCheck < CachePeriod)
            {
                return CachedProgress;
            }

            CachedProgress = await OpenWindowGetFateProgresses();

            lastWindowCheck = DateTime.Now;

            return CachedProgress;
        }

        /// <summary>
        /// Opens the FATE Progress window via the game agent, waits for all zone data to load,
        /// reads the progress array, and closes the window.
        /// </summary>
        /// <returns>An array of <see cref="SharedFateProgress"/> entries, or empty if the window could not be opened.</returns>
        public static async Task<SharedFateProgress[]> OpenWindowGetFateProgresses()
        {
            if (FateProgress.Instance.IsOpen)
            {
                FateProgress.Instance.Close();
                await Coroutine.Wait(10000, () => !FateProgress.Instance.IsOpen);
            }

            AgentFateProgress.Instance.Toggle();

            await Coroutine.Wait(10000, () => FateProgress.Instance.IsOpen);

            if (!FateProgress.Instance.IsOpen)
            {
                return Array.Empty<SharedFateProgress>();
            }

            await Coroutine.Wait(20000, () => AgentFateProgress.Instance.NumberOfLoadedZones == 6);

            if (AgentFateProgress.Instance.NumberOfLoadedZones == 0)
            {
                return Array.Empty<SharedFateProgress>();
            }

            var result = AgentFateProgress.Instance.ProgressArray;

            if (FateProgress.Instance.IsOpen)
            {
                FateProgress.Instance.Close();
            }

            await Coroutine.Wait(10000, () => !FateProgress.Instance.IsOpen);

            return result;
        }
    }
}