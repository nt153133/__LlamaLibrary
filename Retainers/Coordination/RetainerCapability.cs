using System;

namespace LlamaLibrary.Retainers.Coordination
{
    /// <summary>
    /// The units of retainer work that a <see cref="IRetainerSessionParticipant"/> can claim ownership of
    /// for the duration of a session.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Claims exist so that two products which can both perform the same action — collecting gil, entrusting
    /// duplicates, reassigning a venture — agree on exactly one owner per session instead of doing the work
    /// twice or racing each other.
    /// </para>
    /// <para>
    /// A participant should declare <b>everything it is willing and able to do</b>, not just the work it
    /// prefers to own. <see cref="RetainerSessionBroker"/> resolves overlapping claims by
    /// <see cref="IRetainerSessionParticipant.Priority"/> (lowest wins), so declaring a capability you would
    /// only perform as a fallback is correct: you simply lose the claim whenever a higher-priority
    /// participant is present, and win it when it is not.
    /// </para>
    /// <para>
    /// Claim only what your configuration will actually service. Claims are read once per session, so key
    /// them on config rather than on installation: claim <see cref="Entrust"/> when at least one enabled
    /// entry is set to entrust, not merely because you implement entrusting. A capability that is claimed
    /// but never acted on is worse than an unclaimed one — it silences the participant that would have done
    /// the work, and the user sees a feature go quiet with nothing in the log to explain it.
    /// </para>
    /// <para>
    /// Where two products can both service a capability, the winner's per-retainer configuration is
    /// authoritative for it, including a setting that means "leave this retainer alone". Both products
    /// should document which of their switches stop applying when the other is installed, so a user running
    /// both can predict what each one does.
    /// </para>
    /// </remarks>
    [Flags]
    public enum RetainerCapability
    {
        /// <summary>No claim. A participant that only observes, or one whose feature tier is currently locked.</summary>
        None = 0,

        /// <summary>Collecting a completed venture and assigning the next one.</summary>
        Ventures = 1 << 0,

        /// <summary>Moving the retainer's gil into the player's inventory.</summary>
        Gil = 1 << 1,

        /// <summary>Entrusting duplicate stacks from the player's inventory to the retainer.</summary>
        Entrust = 1 << 2,

        /// <summary>Adjusting the prices of the retainer's existing market board listings.</summary>
        Pricing = 1 << 3,

        /// <summary>Posting new items to the market board, and retrieving items in order to post them.</summary>
        Posting = 1 << 4,

        /// <summary>
        /// The retainer-housekeeping group: <see cref="Ventures"/>, <see cref="Gil"/> and <see cref="Entrust"/>.
        /// </summary>
        /// <remarks>
        /// These run on the <b>first pass only</b>, enforced by
        /// <see cref="RetainerSessionContext.IsOwnedBy"/>. Later passes exist to move items out of retainer
        /// bags and post them; re-running housekeeping would entrust those same stacks straight back in.
        /// </remarks>
        Housekeeping = Ventures | Gil | Entrust,

        /// <summary>
        /// The market-operations group: <see cref="Pricing"/> and <see cref="Posting"/>.
        /// </summary>
        Market = Pricing | Posting,
    }
}
