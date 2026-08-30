using System;
using System.Collections.Generic;
using System.Linq;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Retainers.Coordination
{
    /// <summary>
    /// The state shared by every participant in one summoning-bell session: the retainer snapshot, the
    /// resolved capability owners, and the signals participants use to talk to each other.
    /// </summary>
    /// <remarks>
    /// Created by <see cref="RetainerSessionBroker"/> and passed to every participant callback. Participants
    /// read <see cref="Snapshot"/> and <see cref="IsOwnedBy"/>, and write through
    /// <see cref="NotifyItemsMoved"/> and <see cref="RequestAdditionalPass"/>.
    /// </remarks>
    public sealed class RetainerSessionContext
    {
        private readonly Dictionary<RetainerCapability, string> _owners;
        private readonly HashSet<string> _movedItemsBy = new(StringComparer.Ordinal);
        private readonly HashSet<string> _additionalPassBy = new(StringComparer.Ordinal);

        internal RetainerSessionContext(RetainerInfo[] snapshot, Dictionary<RetainerCapability, string> owners, IReadOnlyList<string> requestedBy)
        {
            Snapshot = snapshot;
            _owners = owners;
            RequestedBy = requestedBy;
        }

        /// <summary>
        /// Gets the active retainers as read once at the start of the session, before any participant acted.
        /// </summary>
        /// <value>
        /// The single shared view of retainer state. Every participant sees the same pre-action values, so
        /// one participant reassigning a venture cannot silently invalidate another's reading of it.
        /// </value>
        /// <remarks>
        /// Deliberately not refreshed mid-session. Fields that a participant's own actions change — a venture
        /// end timestamp it just moved, a bag it just filled — are stale by design; read those from the game
        /// when you need them live.
        /// </remarks>
        public RetainerInfo[] Snapshot { get; }

        /// <summary>
        /// Gets the ids of the participants whose <see cref="IRetainerSessionParticipant.WantsSession"/>
        /// returned <see langword="true"/> and therefore caused this trip.
        /// </summary>
        public IReadOnlyList<string> RequestedBy { get; }

        /// <summary>
        /// Gets the current pass number, starting at 1.
        /// </summary>
        /// <value>Incremented when a participant's <see cref="RequestAdditionalPass"/> earns another lap over the retainers.</value>
        public int Pass { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is the first pass over the retainers.
        /// </summary>
        /// <value><see langword="true"/> while <see cref="Pass"/> is 1.</value>
        /// <remarks>
        /// The pass that owns <see cref="RetainerCapability.Housekeeping"/>. Later passes exist so that items
        /// moved between retainers can be posted, and re-running housekeeping on them would undo that work —
        /// entrusting stacks back into a retainer's bag after they were retrieved for exactly the opposite
        /// reason. <see cref="IsOwnedBy"/> enforces this, so a participant that gates on it needs no
        /// pass check of its own.
        /// </remarks>
        public bool IsFirstPass => Pass <= 1;

        /// <summary>
        /// Gets a value indicating whether any participant has reported moving items during this pass.
        /// </summary>
        /// <value><see langword="true"/> if <see cref="NotifyItemsMoved"/> was called since the pass began.</value>
        /// <remarks>
        /// Advisory rather than a precise invalidation protocol: it tells you that a plan built before the
        /// current pass may be stale, not which entries. Rebuild in
        /// <see cref="IRetainerSessionParticipant.OnSessionStarting"/> on every pass rather than relying on
        /// this to be granular.
        /// </remarks>
        public bool ItemsMoved => _movedItemsBy.Count > 0;

        /// <summary>
        /// Gets a value indicating whether another pass over the retainers has been requested.
        /// </summary>
        public bool AdditionalPassRequested => _additionalPassBy.Count > 0;

        /// <summary>
        /// Returns the participant that owns <paramref name="capability"/> for this session.
        /// </summary>
        /// <param name="capability">A single capability flag. Composite values such as <see cref="RetainerCapability.Housekeeping"/> are not owned as a unit and always return <see langword="null"/>.</param>
        /// <returns>
        /// The owning participant's id, or <see langword="null"/> if nobody claimed it or the capability is
        /// not available on the current pass.
        /// </returns>
        public string? OwnerOf(RetainerCapability capability)
        {
            if (!IsAvailableThisPass(capability))
            {
                return null;
            }

            return _owners.TryGetValue(capability, out var owner) ? owner : null;
        }

        /// <summary>
        /// Determines whether <paramref name="participantId"/> owns <paramref name="capability"/> and may act
        /// on it during the current pass.
        /// </summary>
        /// <param name="capability">A single capability flag.</param>
        /// <param name="participantId">The calling participant's <see cref="IRetainerSessionParticipant.Id"/>.</param>
        /// <returns><see langword="true"/> if this participant should perform the work now; otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>
        /// The single gate to check before doing any claimed work. It folds together both reasons to stand
        /// down, so a participant needs no other condition: another participant owns the capability, or the
        /// capability is closed for this pass.
        /// </para>
        /// <para>
        /// A participant that declared the capability but lost it to a higher-priority participant gets
        /// <see langword="false"/> and should skip silently — the work is being done by someone else on this
        /// same trip.
        /// </para>
        /// </remarks>
        public bool IsOwnedBy(RetainerCapability capability, string participantId)
        {
            if (!IsAvailableThisPass(capability))
            {
                return false;
            }

            return _owners.TryGetValue(capability, out var owner) && string.Equals(owner, participantId, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether <paramref name="capability"/> may be acted on during the current pass,
        /// irrespective of who owns it.
        /// </summary>
        /// <param name="capability">A single capability flag.</param>
        /// <returns><see langword="false"/> for housekeeping capabilities after the first pass; otherwise <see langword="true"/>.</returns>
        /// <remarks>
        /// Housekeeping moves items into retainer bags; later passes exist to move items out of them and post
        /// them. Allowing both in one session would let an entrust on pass two undo a retrieval from pass one.
        /// </remarks>
        public bool IsAvailableThisPass(RetainerCapability capability)
        {
            return IsFirstPass || (capability & RetainerCapability.Housekeeping) == 0;
        }

        /// <summary>
        /// Reports that this participant moved items between the player, a retainer or the saddlebag.
        /// </summary>
        /// <param name="participantId">The calling participant's <see cref="IRetainerSessionParticipant.Id"/>.</param>
        /// <remarks>
        /// <para>
        /// <b>Required</b> of any participant that moves an item between the player, a retainer or the
        /// saddlebag — entrusting, retrieving, collecting venture rewards. Sets <see cref="ItemsMoved"/> for
        /// the rest of the pass so participants that planned against inventory contents know their plan
        /// predates a change.
        /// </para>
        /// <para>
        /// Omitting it fails silently rather than loudly: the other participant carries on with a plan that
        /// names the wrong source or destination for a stack you moved.
        /// </para>
        /// </remarks>
        public void NotifyItemsMoved(string participantId)
        {
            _movedItemsBy.Add(participantId);
        }

        /// <summary>
        /// Asks the broker for another lap over the retainers before the list is closed.
        /// </summary>
        /// <param name="participantId">The calling participant's <see cref="IRetainerSessionParticipant.Id"/>.</param>
        /// <remarks>
        /// For work that only becomes possible after a first lap — most obviously retrieving an item from one
        /// retainer in order to post it on another. The broker recomputes the visit set for each pass and
        /// stops at <see cref="RetainerSessionBroker.MaxPasses"/> regardless of further requests.
        /// </remarks>
        public void RequestAdditionalPass(string participantId)
        {
            _additionalPassBy.Add(participantId);
        }

        /// <summary>
        /// Returns the snapshot entry for <paramref name="retainerId"/>.
        /// </summary>
        /// <param name="retainerId">A retainer content id.</param>
        /// <param name="retainer">When this returns <see langword="true"/>, the matching snapshot entry.</param>
        /// <returns><see langword="true"/> if the retainer is in the snapshot; otherwise <see langword="false"/>.</returns>
        public bool TryGetRetainer(ulong retainerId, out RetainerInfo retainer)
        {
            foreach (var candidate in Snapshot)
            {
                if (candidate.Unique == retainerId)
                {
                    retainer = candidate;
                    return true;
                }
            }

            retainer = default;
            return false;
        }

        /// <summary>
        /// Starts a new pass, clearing the per-pass signals.
        /// </summary>
        /// <param name="pass">The pass number, starting at 1.</param>
        internal void BeginPass(int pass)
        {
            Pass = pass;
            _movedItemsBy.Clear();
            _additionalPassBy.Clear();
        }

        /// <summary>
        /// Returns a short description of the resolved claim ownership, for logging.
        /// </summary>
        /// <returns>A comma-separated <c>Capability=Owner</c> list, or a placeholder when nothing was claimed.</returns>
        internal string DescribeClaims()
        {
            return _owners.Count == 0
                ? "none claimed"
                : string.Join(", ", _owners.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
    }
}
