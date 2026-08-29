using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Helpers.Ping;
using LlamaLibrary.Logging;
using LlamaLibrary.Structs;
using RetainerList = LlamaLibrary.RemoteWindows.RetainerList;

namespace LlamaLibrary.Retainers.Coordination
{
    /// <summary>
    /// Owns the trip to the summoning bell so that every product needing retainer work shares one visit
    /// instead of racing for it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Products register an <see cref="IRetainerSessionParticipant"/> and stop navigating to the bell
    /// themselves. Any scheduler may call <see cref="RunIfWanted"/>; a re-entrancy guard means the first
    /// caller in makes the trip and the rest are told a session is already running. Neither product needs
    /// to give up its own hook or task loop, and neither takes a dependency on the other.
    /// </para>
    /// <para>
    /// The design turns on one property: <b>if any participant wants a session, every participant is called
    /// for every retainer visited.</b> A participant therefore never needs a trigger whose only purpose is
    /// to justify a trip — it is guaranteed a seat on any trip that happens.
    /// </para>
    /// <para>
    /// The broker never travels between worlds. Being on the home world is a precondition, not something it
    /// arranges, so a product that wants retainers reached from elsewhere must get there itself. It does walk
    /// the character to a summoning bell, and it does not restore position afterwards: snapshot it before
    /// calling and restore it if <see cref="RetainerSessionResult.Moved"/> is <see langword="true"/>.
    /// </para>
    /// <para>
    /// This type is a published integration surface. Once released it evolves additively only — new members
    /// go on the context object or a new interface, never on
    /// <see cref="IRetainerSessionParticipant"/> — because a consumer binding to an older LlamaLibrary at
    /// runtime fails to load entirely if the interface it compiled against has changed shape.
    /// </para>
    /// </remarks>
    public static class RetainerSessionBroker
    {
        /// <summary>
        /// The hard ceiling on passes over the retainers in a single session, regardless of how many
        /// participants keep calling <see cref="RetainerSessionContext.RequestAdditionalPass"/>.
        /// </summary>
        public const int MaxPasses = 3;

        /// <summary>
        /// How many consecutive callback failures retire a participant.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The streak spans <b>every</b> callback on <see cref="IRetainerSessionParticipant"/>, including
        /// <see cref="IRetainerSessionParticipant.WantsSession"/> and
        /// <see cref="IRetainerSessionParticipant.RetainersOfInterest"/>. A participant that throws on every
        /// poll therefore retires after three polls rather than logging at poll cadence indefinitely. Any
        /// successful callback clears the streak.
        /// </para>
        /// <para>
        /// A retired participant receives no further callbacks and is named in
        /// <see cref="RetainerSessionResult.RetiredParticipants"/>. The retirement is logged once, as a
        /// warning naming the failing stage, so a support log can explain why a feature went quiet rather
        /// than leaving it silently skipped. Calling <see cref="Register"/> again — which a plugin reload
        /// does — clears the retirement and gives the participant a fresh start.
        /// </para>
        /// </remarks>
        public const int MaxConsecutiveFailures = 3;

        private static readonly LLogger Log = new(nameof(RetainerSessionBroker), Colors.Gold);

        private static readonly object SyncRoot = new();

        private static readonly List<IRetainerSessionParticipant> Registered = new();

        /// <summary>
        /// Consecutive failure streaks per participant id. Outlives individual sessions so that failures
        /// during the poll stage, which happens outside any session, still accumulate toward retirement.
        /// </summary>
        private static readonly Dictionary<string, int> ConsecutiveFailures = new(StringComparer.Ordinal);

        /// <summary>
        /// Participants currently retired. Cleared for a participant when it registers again.
        /// </summary>
        private static readonly HashSet<string> RetiredParticipants = new(StringComparer.Ordinal);

        /// <summary>
        /// Exit NPCs for the three Grand Company barracks.
        /// </summary>
        private static readonly uint[] BarracksExitNpcIds = { 2007528, 2006963, 2007530 };

        /// <summary>
        /// The individual capability flags, in the order claims are resolved and reported.
        /// </summary>
        private static readonly RetainerCapability[] SingleCapabilities =
        {
            RetainerCapability.Ventures,
            RetainerCapability.Gil,
            RetainerCapability.Entrust,
            RetainerCapability.Pricing,
            RetainerCapability.Posting,
        };

        private static bool _sessionInProgress;

        /// <summary>
        /// Gets a value indicating whether a session is currently running.
        /// </summary>
        public static bool SessionInProgress
        {
            get
            {
                lock (SyncRoot)
                {
                    return _sessionInProgress;
                }
            }
        }

        /// <summary>
        /// Gets the ids of the currently registered participants, in execution order.
        /// </summary>
        public static IReadOnlyList<string> Participants
        {
            get
            {
                lock (SyncRoot)
                {
                    return Registered.OrderBy(p => p.Priority).Select(p => p.Id).ToList();
                }
            }
        }

        /// <summary>
        /// Registers a participant, replacing any existing registration with the same
        /// <see cref="IRetainerSessionParticipant.Id"/>.
        /// </summary>
        /// <param name="participant">The participant to register.</param>
        /// <exception cref="ArgumentNullException"><paramref name="participant"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><see cref="IRetainerSessionParticipant.Id"/> is null or whitespace.</exception>
        /// <remarks>
        /// <para>
        /// Registration is an upsert keyed on <see cref="IRetainerSessionParticipant.Id"/>, never an append.
        /// This matters because RebornBuddy rebuilds plugins on reload while this registry persists: an
        /// appending registry would leave a stale participant behind after every reload, running its work
        /// twice per retainer and pinning the replaced assembly in memory. Registering the same id again
        /// swaps the old instance out.
        /// </para>
        /// <para>
        /// Registering also clears any retirement and failure streak for that id, so a reload gives a
        /// participant a clean slate. Call this from your plugin's enable path; registering during a session
        /// takes effect on the next one.
        /// </para>
        /// </remarks>
        public static void Register(IRetainerSessionParticipant participant)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant));
            }

            if (string.IsNullOrWhiteSpace(participant.Id))
            {
                throw new ArgumentException("A participant must have a non-empty Id.", nameof(participant));
            }

            bool replaced;
            lock (SyncRoot)
            {
                replaced = Registered.RemoveAll(p => string.Equals(p.Id, participant.Id, StringComparison.Ordinal)) > 0;
                Registered.Add(participant);
                ConsecutiveFailures.Remove(participant.Id);
                RetiredParticipants.Remove(participant.Id);
            }

            Log.Information($"{(replaced ? "Replaced" : "Registered")} participant {participant.Id} (priority {participant.Priority}).");
        }

        /// <summary>
        /// Removes a participant and forgets its failure history.
        /// </summary>
        /// <param name="participantId">The <see cref="IRetainerSessionParticipant.Id"/> to remove.</param>
        /// <returns><see langword="true"/> if a participant was removed; otherwise <see langword="false"/>.</returns>
        /// <remarks>Call from your plugin's disable path. A session already in flight still calls the removed participant.</remarks>
        public static bool Unregister(string participantId)
        {
            int removed;
            lock (SyncRoot)
            {
                removed = Registered.RemoveAll(p => string.Equals(p.Id, participantId, StringComparison.Ordinal));
                ConsecutiveFailures.Remove(participantId);
                RetiredParticipants.Remove(participantId);
            }

            if (removed > 0)
            {
                Log.Information($"Unregistered participant {participantId}.");
            }

            return removed > 0;
        }

        /// <summary>
        /// Polls the participants and, if any of them wants one, makes a single trip to the summoning bell.
        /// </summary>
        /// <returns>The outcome of the session.</returns>
        /// <exception cref="CoroutineStoppedException">
        /// The bot was stopped or the behavior swapped out mid-session. Deliberately propagated rather than
        /// caught, so the coroutine unwinds instead of the trip continuing to drive retainer windows after
        /// the bot was told to stop. The re-entrancy guard is released on the way out.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Safe to call from more than one scheduler and safe to call often. When nobody wants a session the
        /// cost is one memory-read snapshot plus one <see cref="IRetainerSessionParticipant.WantsSession"/>
        /// call per participant, with no window interaction and no network traffic of the broker's own.
        /// </para>
        /// <para>
        /// Apart from <see cref="CoroutineStoppedException"/>, participant exceptions are caught and reported
        /// on the result rather than thrown.
        /// </para>
        /// </remarks>
        public static async Task<RetainerSessionResult> RunIfWanted()
        {
            List<IRetainerSessionParticipant> participants;

            lock (SyncRoot)
            {
                if (_sessionInProgress)
                {
                    return RetainerSessionResult.Skipped(RetainerSessionOutcome.AlreadyRunning, "A retainer session is already running.");
                }

                participants = Registered
                    .Where(p => !RetiredParticipants.Contains(p.Id))
                    .OrderBy(p => p.Priority)
                    .ToList();

                if (participants.Count == 0)
                {
                    return RetainerSessionResult.Skipped(RetainerSessionOutcome.NoParticipants, "No participants are registered.");
                }

                _sessionInProgress = true;
            }

            try
            {
                return await RunSession(participants);
            }
            finally
            {
                // Released in a finally so that a CoroutineStoppedException mid-trip cannot leave the guard
                // held and the broker wedged until RebornBuddy restarts.
                lock (SyncRoot)
                {
                    _sessionInProgress = false;
                }
            }
        }

        private static async Task<RetainerSessionResult> RunSession(List<IRetainerSessionParticipant> participants)
        {
            var failures = new Dictionary<string, int>(StringComparer.Ordinal);

            if (OrderbotHook.DefaultBusyCheck())
            {
                return RetainerSessionResult.Skipped(RetainerSessionOutcome.Busy, "Character is busy.");
            }

            if (!WorldHelper.IsOnHomeWorld)
            {
                return RetainerSessionResult.Skipped(RetainerSessionOutcome.NotOnHomeWorld, "Not on the home world; retainers are only reachable there.");
            }

            // Poll with the cheap read. Once the retainer data pointer is populated this is pure memory
            // access; on a fresh login the first call populates it. Only after a participant commits to a
            // trip is the forced refresh — which costs a retainer data packet and up to six seconds of
            // waiting — worth paying for.
            var pollSnapshot = ActiveRetainers(await HelperFunctions.GetOrderedRetainerArray());
            if (pollSnapshot.Length == 0)
            {
                return RetainerSessionResult.Skipped(RetainerSessionOutcome.NoRetainers, "No active retainers.");
            }

            var requestedBy = new List<string>();
            foreach (var participant in Active(participants))
            {
                try
                {
                    if (await participant.WantsSession(pollSnapshot))
                    {
                        requestedBy.Add(participant.Id);
                    }

                    RecordSuccess(participant.Id);
                }
                catch (CoroutineStoppedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    RecordFailure(participant.Id, "WantsSession", failures);
                    Log.Error($"[{participant.Id}] WantsSession threw, treating as no: {e}");
                }
            }

            if (requestedBy.Count == 0)
            {
                return RetainerSessionResult.Skipped(RetainerSessionOutcome.NoWorkWanted, "No participant wants a session.");
            }

            // Committed to a trip: re-read from the server so the shared snapshot is current.
            var snapshot = ActiveRetainers(await HelperFunctions.GetOrderedRetainerArray(true));
            if (snapshot.Length == 0)
            {
                return RetainerSessionResult.Skipped(RetainerSessionOutcome.NoRetainers, "No active retainers after refresh.");
            }

            // Claims are read here, once, after the trip is committed and before any callback runs. Never at
            // registration time, so a participant whose claims depend on configuration is read as configured
            // now rather than as configured when its plugin was enabled.
            var context = new RetainerSessionContext(snapshot, ResolveClaims(Active(participants)), requestedBy);
            Log.Information($"Session requested by {string.Join(", ", requestedBy)}. Claims: {context.DescribeClaims()}.");

            await GeneralFunctions.StopBusy(dismount: false);

            // Participants pace their window interaction against PingChecker.CurrentPing, which is 0 until
            // something measures it. Left unmeasured, every latency sleep in a participant collapses to
            // nothing and it races the client - writing to an agent before the window it belongs to has
            // settled. Measuring here makes it a property of the trip rather than something each
            // participant has to remember.
            await PingChecker.UpdatePing();
            Log.Debug($"Ping measured at {PingChecker.CurrentPing}ms.");

            if (!await LeaveBarracks())
            {
                return RetainerSessionResult.Failed(RetainerSessionOutcome.BellFailed, "Could not leave the Grand Company barracks.", failures, CurrentlyRetired());
            }

            if (!await HelperFunctions.GoToSummoningBell())
            {
                return RetainerSessionResult.Failed(RetainerSessionOutcome.BellFailed, "Could not reach a summoning bell.", failures, CurrentlyRetired());
            }

            if (!await HelperFunctions.OpenRetainerList())
            {
                return RetainerSessionResult.Failed(RetainerSessionOutcome.BellFailed, "Could not open the retainer list.", failures, CurrentlyRetired());
            }

            var visited = new List<ulong>();
            var passes = 0;

            for (var pass = 1; pass <= MaxPasses; pass++)
            {
                passes = pass;
                context.BeginPass(pass);

                foreach (var participant in Active(participants))
                {
                    await Invoke(failures, participant, "OnSessionStarting", () => participant.OnSessionStarting(context));
                }

                var visitSet = BuildVisitSet(Active(participants), context, failures);
                if (visitSet.Count == 0)
                {
                    Log.Information(pass == 1 ? "No retainer needs visiting." : $"Pass {pass} has nothing left to do.");
                    break;
                }

                Log.Information($"Pass {pass}: visiting {visitSet.Count} retainer(s).");

                foreach (var retainerId in visitSet)
                {
                    if (!context.TryGetRetainer(retainerId, out var retainer))
                    {
                        continue;
                    }

                    if (!await RetainerRoutine.SelectRetainer(retainerId))
                    {
                        Log.Error($"Could not select retainer {retainer.Name}, skipping it.");
                        continue;
                    }

                    if (!visited.Contains(retainerId))
                    {
                        visited.Add(retainerId);
                    }

                    foreach (var participant in Active(participants))
                    {
                        await Invoke(failures, participant, $"OnRetainer({retainer.Name})", () => participant.OnRetainer(context, retainer));
                    }

                    await RetainerRoutine.DeSelectRetainer();
                }

                if (!context.AdditionalPassRequested)
                {
                    break;
                }

                if (pass == MaxPasses)
                {
                    Log.Warning($"Another pass was requested but the {MaxPasses}-pass ceiling was reached; stopping here.");
                }
            }

            var closed = await CloseList();

            foreach (var participant in Active(participants))
            {
                await Invoke(failures, participant, "OnSessionEnded", () => participant.OnSessionEnded(context));
            }

            var result = RetainerSessionResult.Ran(
                closed ? RetainerSessionOutcome.Completed : RetainerSessionOutcome.CloseFailed,
                visited,
                passes,
                failures,
                CurrentlyRetired());

            Log.Information($"Session finished — {result}");
            return result;
        }

        /// <summary>
        /// Filters a retainer array down to the active entries.
        /// </summary>
        /// <param name="retainers">The array to filter.</param>
        /// <returns>Only the retainers marked active.</returns>
        private static RetainerInfo[] ActiveRetainers(RetainerInfo[] retainers)
        {
            return retainers.Where(r => r.Active).ToArray();
        }

        /// <summary>
        /// Filters out participants retired since the session started.
        /// </summary>
        /// <param name="participants">The session participants, in priority order.</param>
        /// <returns>The participants still receiving callbacks.</returns>
        private static List<IRetainerSessionParticipant> Active(List<IRetainerSessionParticipant> participants)
        {
            lock (SyncRoot)
            {
                return participants.Where(p => !RetiredParticipants.Contains(p.Id)).ToList();
            }
        }

        /// <summary>
        /// Clears a participant's consecutive failure streak after a clean callback.
        /// </summary>
        /// <param name="participantId">The participant id.</param>
        private static void RecordSuccess(string participantId)
        {
            lock (SyncRoot)
            {
                ConsecutiveFailures.Remove(participantId);
            }
        }

        /// <summary>
        /// Records a failed callback, retiring the participant once it hits
        /// <see cref="MaxConsecutiveFailures"/> in a row.
        /// </summary>
        /// <param name="participantId">The participant id.</param>
        /// <param name="stage">The callback that failed, named in the retirement warning.</param>
        /// <param name="sessionFailures">Per-session tally reported on the result.</param>
        private static void RecordFailure(string participantId, string stage, Dictionary<string, int> sessionFailures)
        {
            sessionFailures[participantId] = sessionFailures.TryGetValue(participantId, out var seen) ? seen + 1 : 1;

            bool retire;
            int streak;
            lock (SyncRoot)
            {
                streak = (ConsecutiveFailures.TryGetValue(participantId, out var consecutive) ? consecutive : 0) + 1;
                ConsecutiveFailures[participantId] = streak;
                retire = streak >= MaxConsecutiveFailures && RetiredParticipants.Add(participantId);
            }

            if (retire)
            {
                Log.Warning($"Retiring participant {participantId} after {streak} consecutive failures, most recently in {stage}. Re-enable its plugin to give it another try.");
            }
        }

        /// <summary>
        /// Snapshots the currently retired participant ids for a result.
        /// </summary>
        /// <returns>The retired ids.</returns>
        private static List<string> CurrentlyRetired()
        {
            lock (SyncRoot)
            {
                return RetiredParticipants.ToList();
            }
        }

        /// <summary>
        /// Assigns each capability to the lowest-priority participant that claims it.
        /// </summary>
        /// <param name="participants">Participants already ordered by priority, ascending.</param>
        /// <returns>A map of capability to owning participant id, omitting capabilities nobody claimed.</returns>
        private static Dictionary<RetainerCapability, string> ResolveClaims(List<IRetainerSessionParticipant> participants)
        {
            var owners = new Dictionary<RetainerCapability, string>();

            foreach (var capability in SingleCapabilities)
            {
                foreach (var participant in participants)
                {
                    RetainerCapability claims;
                    try
                    {
                        claims = participant.Claims;
                    }
                    catch (CoroutineStoppedException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[{participant.Id}] Claims threw, treating as None: {e}");
                        continue;
                    }

                    if ((claims & capability) == capability)
                    {
                        owners[capability] = participant.Id;
                        break;
                    }
                }
            }

            return owners;
        }

        /// <summary>
        /// Builds the union of every participant's retainers of interest, in retainer-list order.
        /// </summary>
        /// <param name="participants">The participants still active this session.</param>
        /// <param name="context">The session context, supplying the snapshot and its ordering.</param>
        /// <param name="failures">Failure tally to record participants that threw.</param>
        /// <returns>Content ids to visit this pass.</returns>
        private static List<ulong> BuildVisitSet(List<IRetainerSessionParticipant> participants, RetainerSessionContext context, Dictionary<string, int> failures)
        {
            var interested = new HashSet<ulong>();

            foreach (var participant in participants)
            {
                try
                {
                    var wanted = participant.RetainersOfInterest(context.Snapshot);
                    if (wanted != null)
                    {
                        foreach (var retainerId in wanted)
                        {
                            interested.Add(retainerId);
                        }
                    }

                    RecordSuccess(participant.Id);
                }
                catch (CoroutineStoppedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    RecordFailure(participant.Id, "RetainersOfInterest", failures);
                    Log.Error($"[{participant.Id}] RetainersOfInterest threw, contributing nothing: {e}");
                }
            }

            return context.Snapshot.Where(r => interested.Contains(r.Unique)).Select(r => r.Unique).ToList();
        }

        /// <summary>
        /// Runs one participant callback, containing any exception so the other participants are unaffected.
        /// </summary>
        /// <param name="failures">Failure tally.</param>
        /// <param name="participant">The participant being invoked.</param>
        /// <param name="stage">A short label used in the log line.</param>
        /// <param name="action">The callback.</param>
        /// <returns><see langword="true"/> if the callback completed; <see langword="false"/> if it threw.</returns>
        /// <exception cref="CoroutineStoppedException">The bot is stopping; rethrown so the coroutine unwinds.</exception>
        private static async Task<bool> Invoke(Dictionary<string, int> failures, IRetainerSessionParticipant participant, string stage, Func<Task> action)
        {
            try
            {
                await action();
                RecordSuccess(participant.Id);
                return true;
            }
            catch (CoroutineStoppedException)
            {
                throw;
            }
            catch (Exception e)
            {
                RecordFailure(participant.Id, stage, failures);
                Log.Error($"[{participant.Id}] {stage} threw: {e}");
                return false;
            }
        }

        /// <summary>
        /// Leaves the Grand Company barracks if the character is inside, since no summoning bell is
        /// reachable from there.
        /// </summary>
        /// <returns><see langword="true"/> if the character is out of the barracks; otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// Lives here rather than in a participant because it is a precondition for reaching a bell at all.
        /// Whichever product triggers the session, the trip has to survive the character being parked in the
        /// barracks — and <see cref="HelperFunctions.GoToSummoningBell"/> has no handling of its own.
        /// </remarks>
        private static async Task<bool> LeaveBarracks()
        {
            if (!GrandCompanyHelper.IsInBarracks)
            {
                return true;
            }

            Log.Information("In the Grand Company barracks, leaving so a summoning bell is reachable.");

            var exit = GameObjectManager.GetObjectsByNPCIds<GameObject>(BarracksExitNpcIds).FirstOrDefault();
            if (exit == null)
            {
                Log.Error("Could not find the barracks exit.");
                return false;
            }

            if (!await NavigationHelper.InteractWithNpc(exit))
            {
                Log.Error("Could not interact with the barracks exit.");
                return false;
            }

            if (!await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
            {
                Log.Error("The barracks exit confirmation never opened.");
                return false;
            }

            SelectYesno.Yes();

            await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading);
            Log.Information("Waiting for the zone to finish loading.");
            await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);

            return !GrandCompanyHelper.IsInBarracks;
        }

        /// <summary>
        /// Closes the retainer list, falling back to a direct close if the helper cannot.
        /// </summary>
        /// <returns><see langword="true"/> if the list is closed; otherwise <see langword="false"/>.</returns>
        private static async Task<bool> CloseList()
        {
            if (!RetainerList.Instance.IsOpen)
            {
                return true;
            }

            if (await HelperFunctions.CloseRetainerList())
            {
                return true;
            }

            Log.Error("Could not close the retainer list, trying a direct close.");
            RetainerList.Instance.Close();

            return await Coroutine.Wait(5000, () => !RetainerList.Instance.IsOpen);
        }
    }
}
