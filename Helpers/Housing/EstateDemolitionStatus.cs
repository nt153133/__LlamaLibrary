using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.Structs.Housing;

namespace LlamaLibrary.Helpers.Housing;

/// <summary>
/// Provides typed access to the estate auto-demolition data maintained by
/// AgentContentsTimer, following the same direct-memory pattern as SquadronStatus.
/// </summary>
public static class EstateDemolitionStatus
{
    private static readonly TimeSpan CachePeriod = TimeSpan.FromMinutes(1);
    private static DateTime _lastCheckUtc;
    private static EstateDemolitionSnapshot? _cachedSnapshot;

    /// <summary>Forces an immediate read from the live Contents Info agent.</summary>
    /// <returns>The newly decoded estate demolition snapshot.</returns>
    public static EstateDemolitionSnapshot Update()
    {
        _cachedSnapshot = AgentContentsInfo.Instance.ReadEstateDemolitionSnapshot();
        _lastCheckUtc = DateTime.UtcNow;
        return _cachedSnapshot;
    }

    /// <summary>Gets the latest cached snapshot, refreshing it after one minute.</summary>
    public static EstateDemolitionSnapshot Status =>
        _cachedSnapshot != null && DateTime.UtcNow - _lastCheckUtc < CachePeriod
            ? _cachedSnapshot
            : Update();

    /// <summary>Gets a snapshot, optionally bypassing the one-minute cache.</summary>
    /// <param name="forceRefresh">Whether to read the live client data even when the cache is current.</param>
    /// <returns>The cached or newly decoded estate demolition snapshot.</returns>
    public static EstateDemolitionSnapshot GetSnapshot(bool forceRefresh = false) =>
        forceRefresh ? Update() : Status;

    /// <summary>
    /// Waits for the server-pushed agent data to report that the specified estate
    /// is no longer scheduled for auto-demolition.
    /// </summary>
    /// <param name="estateType">The estate whose cancellation state should be verified.</param>
    /// <param name="timeoutMilliseconds">The maximum time to poll refreshed client data.</param>
    /// <returns>The final cancellation result and the snapshot used to determine it.</returns>
    public static async Task<EstateDemolitionResult> VerifyCancellationAsync(
        EstateType estateType,
        int timeoutMilliseconds = 15000)
    {
        EstateDemolitionSnapshot snapshot = Update();
        var canceled = snapshot.IsValid &&
                       snapshot.GetEntry(estateType)?.State == EstateDemolitionState.NotScheduled;

        if (!canceled)
        {
            var nextPollUtc = DateTime.MinValue;
            await Coroutine.Wait(timeoutMilliseconds, () =>
            {
                if (DateTime.UtcNow < nextPollUtc)
                {
                    return false;
                }

                nextPollUtc = DateTime.UtcNow.AddSeconds(1);
                snapshot = Update();
                return snapshot.IsValid &&
                       snapshot.GetEntry(estateType)?.State == EstateDemolitionState.NotScheduled;
            });

            canceled = snapshot.IsValid &&
                       snapshot.GetEntry(estateType)?.State == EstateDemolitionState.NotScheduled;
        }

        return new EstateDemolitionResult(estateType, canceled, snapshot);
    }
}
