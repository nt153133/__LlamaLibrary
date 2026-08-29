using System.Collections.Generic;
using System.Threading.Tasks;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Retainers.Coordination
{
    /// <summary>
    /// A consumer of a shared summoning-bell trip. Implement this to have work performed at the retainer
    /// window without owning the trip to the bell.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Register with <see cref="RetainerSessionBroker.Register"/>. The broker makes at most one trip at a
    /// time, opens the retainer list once, and calls <see cref="OnRetainer"/> for every participant on every
    /// retainer it visits.
    /// </para>
    /// <para>
    /// The important consequence: <see cref="WantsSession"/> returning <see langword="false"/> does
    /// <b>not</b> exclude you from the trip. If any other participant wants a session you are still called
    /// for every retainer, so you never need a trigger whose only purpose is to justify a trip you would
    /// have wanted anyway.
    /// </para>
    /// <para>
    /// Every callback is invoked inside a try/catch. Throwing fails only your participant for that stage —
    /// it is logged and recorded on <see cref="RetainerSessionResult.ParticipantFailures"/>, and the other
    /// participants continue unaffected. The one exception deliberately not caught is
    /// <see cref="Buddy.Coroutines.CoroutineStoppedException"/>: it means the bot is stopping, so it
    /// propagates and unwinds the trip rather than letting it keep driving retainer windows. Do not swallow
    /// it in your own callbacks either. Repeated failures retire a participant for the session — see
    /// <see cref="RetainerSessionBroker.MaxConsecutiveFailures"/>.
    /// </para>
    /// <para>
    /// <b>If you move items, you must say so.</b> Call
    /// <see cref="RetainerSessionContext.NotifyItemsMoved"/> whenever a callback moves an item between the
    /// player, a retainer or the saddlebag — entrusting, retrieving, collecting venture rewards, anything
    /// that changes where a stack lives. Other participants plan against inventory contents, and a plan
    /// built before your move is wrong afterwards. Skipping this call does not fail loudly; it silently
    /// corrupts someone else's posting plan, which is the single easiest way to break a shared trip.
    /// </para>
    /// <para>
    /// Register with <see cref="RetainerSessionBroker.Register"/> on enable and
    /// <see cref="RetainerSessionBroker.Unregister"/> on disable. Registration is an upsert keyed on
    /// <see cref="Id"/>, so a plugin reload that re-registers replaces the old instance rather than adding a
    /// second one.
    /// </para>
    /// <para>
    /// <b>This interface is frozen once published.</b> A consumer that compiles against one LlamaLibrary
    /// version and binds to another at runtime fails to load outright — not gracefully — if the interface
    /// changed shape, taking its whole botbase with it. New capabilities are therefore added to
    /// <see cref="RetainerSessionContext"/> or to a separate interface, never as new members here. Document
    /// a minimum LlamaLibrary version for any build that implements this.
    /// </para>
    /// </remarks>
    public interface IRetainerSessionParticipant
    {
        /// <summary>
        /// Gets a stable identifier for this participant, unique across all registrations.
        /// </summary>
        /// <value>Used for claim ownership, logging and <see cref="RetainerSessionBroker.Unregister"/>. Use the product name.</value>
        string Id { get; }

        /// <summary>
        /// Gets this participant's ordering weight. Lower runs first.
        /// </summary>
        /// <value>
        /// Serves two purposes at once, and they agree: it is the order <see cref="OnRetainer"/> is called in
        /// while a retainer is selected, and it is how overlapping <see cref="Claims"/> are resolved — the
        /// lowest-priority participant claiming a capability owns it.
        /// </value>
        /// <remarks>
        /// <para>
        /// Retainer housekeeping must run before market operations, because entrusting or retrieving items
        /// invalidates any posting plan built earlier in the session. Participants that move items should
        /// therefore sit at a lower priority than participants that post them.
        /// </para>
        /// <para>
        /// The canonical order of work within one retainer is: <b>ventures, entrust, gil, pricing, retrieve,
        /// post</b>. Ventures precede entrust deliberately — venture rewards land in the <i>player's</i>
        /// inventory, not the retainer's bag, so collecting first lets fresh loot that stacks with the
        /// retainer's existing stock be entrusted on the same visit.
        /// </para>
        /// </remarks>
        int Priority { get; }

        /// <summary>
        /// Gets the work this participant is willing and able to perform this session.
        /// </summary>
        /// <value>
        /// May vary with settings: return <see cref="RetainerCapability.None"/> for work you currently
        /// cannot do and the claim passes to the next participant that wants it.
        /// </value>
        /// <remarks>
        /// <b>Read once per session</b>, after a participant has committed to a trip and before any other
        /// callback runs — never cached at registration time. That is part of this contract, not an
        /// implementation detail: it is what lets a participant key its claims on configuration that the
        /// user can change between sessions without re-registering.
        /// </remarks>
        RetainerCapability Claims { get; }

        /// <summary>
        /// Asks whether this participant has enough work to justify a trip to the summoning bell.
        /// </summary>
        /// <param name="snapshot">Active retainers as read at the start of this evaluation.</param>
        /// <returns>
        /// <see langword="true"/> if a trip is worth making. Returning <see langword="false"/> does not
        /// exclude this participant from a trip another participant triggers.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Called on every broker poll, so keep it cheap. Settings and plain memory reads — a currency count,
        /// an inventory slot tally — are fine; window interaction, navigation and network traffic are not.
        /// </para>
        /// <para>
        /// Prefer <paramref name="snapshot"/> over a separately persisted copy of retainer state. The
        /// snapshot is the one view all participants share; a private copy can be invalidated by another
        /// participant's actions without you ever observing the change, which is the precise failure this
        /// broker exists to remove.
        /// </para>
        /// <para>
        /// This snapshot is the cheap read. Once a trip is committed the broker forces a fresh one from the
        /// server, and that — not this — is what <see cref="RetainerSessionContext.Snapshot"/> carries.
        /// </para>
        /// </remarks>
        Task<bool> WantsSession(RetainerInfo[] snapshot);

        /// <summary>
        /// Names the retainers this participant needs selected.
        /// </summary>
        /// <param name="snapshot">Active retainers as read at the start of the session.</param>
        /// <returns>Content ids (<see cref="RetainerInfo.Unique"/>) of retainers of interest; may be empty.</returns>
        /// <remarks>
        /// The broker visits the union across all participants, in retainer-list order, so expect
        /// <see cref="OnRetainer"/> for retainers you did not ask for. Return early and cheaply in that case.
        /// </remarks>
        IEnumerable<ulong> RetainersOfInterest(RetainerInfo[] snapshot);

        /// <summary>
        /// Called once per pass, after the retainer list is open and before the first retainer is selected.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>A task that completes when this participant is ready for the pass.</returns>
        /// <remarks>
        /// The place to build any plan that spans retainers. Rebuild it on every pass rather than only the
        /// first: <see cref="RetainerSessionContext.Pass"/> tells you which pass you are in, and earlier
        /// passes may have moved items between retainers.
        /// </remarks>
        Task OnSessionStarting(RetainerSessionContext context);

        /// <summary>
        /// Called with <paramref name="retainer"/> already selected and its menu open.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="retainer">The selected retainer, taken from the session snapshot.</param>
        /// <returns>A task that completes when this participant is done with this retainer.</returns>
        /// <remarks>
        /// <para>
        /// Check <see cref="RetainerSessionContext.IsOwnedBy"/> before performing any claimed work. It is the
        /// only gate you need: it covers both another participant owning the capability and the capability
        /// being closed for the current pass. Housekeeping runs on the first pass only, and
        /// <see cref="RetainerSessionContext.IsOwnedBy"/> enforces that for you.
        /// </para>
        /// <para>
        /// Do not deselect the retainer or close the list — the broker owns the window. Call
        /// <see cref="RetainerSessionContext.NotifyItemsMoved"/> if you move items, and
        /// <see cref="RetainerSessionContext.RequestAdditionalPass"/> if that means another lap is needed.
        /// </para>
        /// </remarks>
        Task OnRetainer(RetainerSessionContext context, RetainerInfo retainer);

        /// <summary>
        /// Called once after the retainer list is closed, whether or not the session completed cleanly.
        /// </summary>
        /// <param name="context">The session context, carrying the final pass count.</param>
        /// <returns>A task that completes when this participant has finished tidying up.</returns>
        /// <remarks>
        /// The place for post-session work that does not need a retainer selected — combining stacks,
        /// clearing caches, stamping a last-run time. The player may no longer be at the bell.
        /// </remarks>
        Task OnSessionEnded(RetainerSessionContext context);
    }
}
