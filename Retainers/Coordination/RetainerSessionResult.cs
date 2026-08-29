using System;
using System.Collections.Generic;
using System.Linq;

namespace LlamaLibrary.Retainers.Coordination
{
    /// <summary>
    /// Why a call to <see cref="RetainerSessionBroker.RunIfWanted"/> ended the way it did.
    /// </summary>
    public enum RetainerSessionOutcome
    {
        /// <summary>The session ran to completion and the retainer list was closed.</summary>
        Completed,

        /// <summary>Nobody is registered with the broker.</summary>
        NoParticipants,

        /// <summary>A session is already running. The caller should try again later.</summary>
        AlreadyRunning,

        /// <summary>The player was moving, occupied, in combat, dead, in an instance or in a FATE.</summary>
        Busy,

        /// <summary>No active retainers were found.</summary>
        NoRetainers,

        /// <summary>Every participant declined; no trip was worth making.</summary>
        NoWorkWanted,

        /// <summary>
        /// The player is not on their home world, where retainers live.
        /// </summary>
        /// <remarks>
        /// The broker never travels between worlds — it treats being home as a precondition and skips
        /// otherwise, leaving world travel to whichever product had a reason to move.
        /// </remarks>
        NotOnHomeWorld,

        /// <summary>Reached the home world but could not open the retainer list at a summoning bell.</summary>
        BellFailed,

        /// <summary>The session ran but the retainer list could not be closed afterwards.</summary>
        CloseFailed,
    }

    /// <summary>
    /// The outcome of one <see cref="RetainerSessionBroker"/> session.
    /// </summary>
    public sealed class RetainerSessionResult
    {
        private RetainerSessionResult(RetainerSessionOutcome outcome, string message, IReadOnlyList<ulong> retainersVisited, int passes, IReadOnlyDictionary<string, int> participantFailures, IReadOnlyList<string> retiredParticipants)
        {
            Outcome = outcome;
            Message = message;
            RetainersVisited = retainersVisited;
            Passes = passes;
            ParticipantFailures = participantFailures;
            RetiredParticipants = retiredParticipants;
        }

        /// <summary>Gets the reason the session ended.</summary>
        public RetainerSessionOutcome Outcome { get; }

        /// <summary>Gets a human-readable summary, suitable for a log line.</summary>
        public string Message { get; }

        /// <summary>Gets the content ids of the retainers that were selected at least once.</summary>
        public IReadOnlyList<ulong> RetainersVisited { get; }

        /// <summary>Gets the number of passes made over the retainers.</summary>
        public int Passes { get; }

        /// <summary>
        /// Gets the number of callbacks that threw, keyed by participant id.
        /// </summary>
        /// <value>Empty when every participant completed cleanly. A participant appearing here still had its other callbacks invoked, unless it was retired.</value>
        public IReadOnlyDictionary<string, int> ParticipantFailures { get; }

        /// <summary>
        /// Gets the participants that hit <see cref="RetainerSessionBroker.MaxConsecutiveFailures"/> and were
        /// dropped for the rest of the session.
        /// </summary>
        /// <value>
        /// Empty in the normal case. A retired participant receives no further callbacks — in this session or
        /// later ones — until its plugin registers again, and the retirement was logged once with its reason.
        /// </value>
        public IReadOnlyList<string> RetiredParticipants { get; }

        /// <summary>
        /// Gets a value indicating whether a trip was made and completed.
        /// </summary>
        /// <value><see langword="true"/> only for <see cref="RetainerSessionOutcome.Completed"/>, regardless of individual participant failures.</value>
        public bool Success => Outcome == RetainerSessionOutcome.Completed;

        /// <summary>
        /// Gets a value indicating whether the player was taken to a summoning bell.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the session got far enough to walk the character to a summoning bell,
        /// so the caller knows whether it needs to return to where it was. Never indicates world travel —
        /// the broker does not move between worlds.
        /// </value>
        public bool Moved => Outcome is RetainerSessionOutcome.Completed or RetainerSessionOutcome.BellFailed or RetainerSessionOutcome.CloseFailed;

        /// <summary>
        /// Creates a result for a session that ended before the character moved.
        /// </summary>
        /// <param name="outcome">The reason nothing ran.</param>
        /// <param name="message">A human-readable summary.</param>
        /// <returns>A result with no retainers visited and no passes.</returns>
        internal static RetainerSessionResult Skipped(RetainerSessionOutcome outcome, string message)
        {
            return new RetainerSessionResult(outcome, message, Array.Empty<ulong>(), 0, new Dictionary<string, int>(StringComparer.Ordinal), Array.Empty<string>());
        }

        /// <summary>
        /// Creates a result for a session that started but could not finish.
        /// </summary>
        /// <param name="outcome">The reason the session stopped.</param>
        /// <param name="message">A human-readable summary.</param>
        /// <param name="failures">Participant callback failures recorded so far.</param>
        /// <param name="retired">Participants dropped for the session.</param>
        /// <returns>A failed result carrying whatever progress was made.</returns>
        internal static RetainerSessionResult Failed(RetainerSessionOutcome outcome, string message, IReadOnlyDictionary<string, int> failures, IReadOnlyList<string> retired)
        {
            return new RetainerSessionResult(outcome, message, Array.Empty<ulong>(), 0, failures, retired);
        }

        /// <summary>
        /// Creates a result for a session that ran to completion.
        /// </summary>
        /// <param name="outcome">Normally <see cref="RetainerSessionOutcome.Completed"/>, or <see cref="RetainerSessionOutcome.CloseFailed"/> if the window would not close.</param>
        /// <param name="visited">The retainers selected during the session.</param>
        /// <param name="passes">The number of passes made.</param>
        /// <param name="failures">Participant callback failures.</param>
        /// <param name="retired">Participants dropped for the session.</param>
        /// <returns>A populated result.</returns>
        internal static RetainerSessionResult Ran(RetainerSessionOutcome outcome, IReadOnlyList<ulong> visited, int passes, IReadOnlyDictionary<string, int> failures, IReadOnlyList<string> retired)
        {
            var failureText = failures.Count == 0
                ? string.Empty
                : $", {failures.Sum(kvp => kvp.Value)} participant failure(s): {string.Join(", ", failures.Select(kvp => $"{kvp.Key}x{kvp.Value}"))}";

            var retiredText = retired.Count == 0
                ? string.Empty
                : $", retired: {string.Join(", ", retired)}";

            return new RetainerSessionResult(
                outcome,
                $"{visited.Count} retainer(s) over {passes} pass(es){failureText}{retiredText}",
                visited,
                passes,
                failures,
                retired);
        }

        /// <summary>
        /// Returns the outcome and message.
        /// </summary>
        /// <returns>A string of the form <c>Outcome: message</c>.</returns>
        public override string ToString()
        {
            return $"{Outcome}: {Message}";
        }
    }
}
