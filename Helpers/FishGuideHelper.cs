using System;
using System.Linq;
using System.Threading.Tasks;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Provides cached access to the player's fishing guide progress data
/// via the <c>AgentFishGuide2</c> game agent, which tracks which fish
/// have been caught across all fishing locations.
/// </summary>
public class FishGuideHelper
{
    private static DateTime lastWindowCheck;

    /// <summary>Most recently read array of fishing guide entries, cached for up to 30 seconds.</summary>
    public static FishGuide2Item[] CachedProgress;

    private static readonly TimeSpan CachePeriod = new(0, 0, 30);

    /// <summary>
    /// Returns the cached fish guide progress if fresh; otherwise opens the game agent,
    /// reads the full list, and updates the cache.
    /// </summary>
    /// <returns>An array of all <see cref="FishGuide2Item"/> entries from the fishing guide.</returns>
    public static async Task<FishGuide2Item[]> CachedRead()
    {
        if (DateTime.Now - lastWindowCheck < CachePeriod)
        {
            return CachedProgress;
        }

        CachedProgress = await AgentFishGuide2.Instance.GetFishList();

        lastWindowCheck = DateTime.Now;

        return CachedProgress;
    }

    /// <summary>
    /// Returns the cached entry for the specified fish, refreshing the cache if needed.
    /// </summary>
    /// <param name="fishId">The fish item row ID to look up.</param>
    /// <returns>The matching <see cref="FishGuide2Item"/>, or default if not found.</returns>
    public static async Task<FishGuide2Item> GetFish(int fishId)
    {
        return (await CachedRead()).FirstOrDefault(i => i.FishItem == fishId);
    }

    /// <summary>
    /// Synchronously looks up a fish entry from the existing cache without refreshing.
    /// Only call after <see cref="CachedRead"/> has been awaited at least once.
    /// </summary>
    /// <param name="fishId">The fish item row ID to look up.</param>
    /// <returns>The matching <see cref="FishGuide2Item"/>, or default if not found.</returns>
    public static FishGuide2Item GetFishSync(int fishId)
    {
        return CachedProgress.FirstOrDefault(i => i.FishItem == fishId);
    }
}