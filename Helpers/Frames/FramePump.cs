using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.Frames;

/// <summary>
/// Centralized scheduler for game-memory reads and injected calls issued from UI code.
/// One dedicated background thread services all work inside a single shared
/// <c>Core.Memory.AcquireFrame(true)</c> scope per tick, so N windows share one game-frame
/// hold instead of stacking their own locks.
/// <para>
/// Recurring subscriptions tick only while <c>TreeRoot.IsRunning</c> is false. While the bot
/// runs, call <see cref="PulseFromBotThread"/> from a plugin or botbase pulse to keep the same
/// subscriptions fresh from the bot thread; otherwise panels simply pause during runs.
/// </para>
/// <para>
/// Readers run inside the frame lock and must copy to dead data (no live wrappers in the
/// result). Same-thread nested frame locks (e.g. Lua.*) are verified reentrant on beta
/// 1.0.9003.0, but they lengthen the frame hold — keep hot readers to plain manager/object
/// reads. Never fan out to other threads from inside the lock (the GameWorld.GetTriangles
/// hazard: cross-thread nesting deadlocks).
/// </para>
/// </summary>
public static class FramePump
{
    private static readonly LLogger Log = new LLogger("FramePump", Colors.MediumPurple);
    private static readonly object Sync = new object();
    private static readonly List<FrameSubscription> Subscriptions = new List<FrameSubscription>();
    private static readonly Queue<OneShotWorkItem> OneShots = new Queue<OneShotWorkItem>();
    private static readonly AutoResetEvent Wake = new AutoResetEvent(false);

    private static Thread? _thread;

    /// <summary>Gets or sets the tick interval used by subscriptions that do not specify their own.</summary>
    public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Gets or sets how long a subscription sleeps after its reader throws, to avoid log spam at title/loading screens.</summary>
    public static TimeSpan FaultBackoff { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Queues <paramref name="work"/> to run inside a frame lock on the pump thread.
    /// Safe to call (and await) from the UI thread and from worker threads. When called from
    /// inside a coroutine, the work executes inline — the pulse already provides frame context,
    /// and awaiting a TPL task from a coroutine would fault it.
    /// </summary>
    public static Task RunAsync(Action work, CancellationToken ct = default)
    {
        if (work == null)
        {
            throw new ArgumentNullException(nameof(work));
        }

        return RunAsync<object?>(
            () =>
            {
                work();
                return null;
            },
            ct);
    }

    /// <summary>
    /// Queues <paramref name="work"/> to run inside a frame lock on the pump thread and returns its result.
    /// The cancellation token only prevents un-started work from running; it cannot abort a read
    /// already blocked in frame acquisition (e.g. during a loading screen).
    /// </summary>
    public static async Task<T> RunAsync<T>(Func<T> work, CancellationToken ct = default)
    {
        if (work == null)
        {
            throw new ArgumentNullException(nameof(work));
        }

        ct.ThrowIfCancellationRequested();

        if (Coroutine.Current != null)
        {
            return work();
        }

        var item = new OneShotWorkItem(() => work(), ct);
        lock (Sync)
        {
            OneShots.Enqueue(item);
            EnsureThread();
        }

        Wake.Set();
        var result = await item.Task.ConfigureAwait(false);
        return (T)result!;
    }

    /// <summary>
    /// Registers a recurring reader for a live panel. <paramref name="readInFrame"/> executes inside
    /// the shared frame lock on the pump thread and must return dead data (primitives/strings copied
    /// out of the game — never live wrappers). <paramref name="applyResult"/> is posted to the
    /// SynchronizationContext current at registration time, so subscribe from your UI thread
    /// (WPF Loaded / WinForms HandleCreated) and dispose the returned subscription on unload.
    /// </summary>
    public static FrameSubscription Subscribe<T>(string owner, Func<T> readInFrame, Action<T> applyResult, TimeSpan? interval = null)
    {
        if (readInFrame == null)
        {
            throw new ArgumentNullException(nameof(readInFrame));
        }

        if (applyResult == null)
        {
            throw new ArgumentNullException(nameof(applyResult));
        }

        var sub = new FrameSubscription(
            owner,
            () => (object?)readInFrame(),
            o => applyResult((T)o!),
            interval ?? DefaultInterval,
            SynchronizationContext.Current);

        lock (Sync)
        {
            Subscriptions.Add(sub);
            EnsureThread();
        }

        Wake.Set();
        Log.Debug($"[{owner}] subscribed ({sub.Interval.TotalMilliseconds:F0}ms)");
        return sub;
    }

    /// <summary>
    /// Runs due subscriptions from the bot thread while the bot is running, using the pulse's own
    /// frame context (no additional lock is taken). Wire this into a plugin's OnPulse (or a botbase
    /// pulse) if panels should stay live during runs. A benign double-read can occur for one interval
    /// at the exact stop/start transition; both sides publish consistent snapshots.
    /// </summary>
    public static void PulseFromBotThread()
    {
        if (!TreeRoot.IsRunning)
        {
            return;
        }

        List<FrameSubscription> due;
        lock (Sync)
        {
            if (Subscriptions.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            due = Subscriptions.Where(s => s.IsDue(now)).ToList();
        }

        foreach (var sub in due)
        {
            if (sub.TryRead(Log, FaultBackoff, out var value))
            {
                sub.Publish(value, Log);
            }
        }
    }

    /// <summary>
    /// Debug guard: call at the top of any code path that reads game memory so an accidental
    /// UI-thread read becomes a loud failure instead of a mystery freeze. No-op in release builds.
    /// </summary>
    [Conditional("DEBUG")]
    public static void AssertNotUiThread(string what = "game memory access")
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher != null && dispatcher.CheckAccess())
        {
            Log.Error($"{what} on the WPF dispatcher thread — this freezes the UI on .NET 10.");
            Debug.Fail($"{what} on the WPF dispatcher thread");
        }
    }

    internal static void Remove(FrameSubscription sub)
    {
        lock (Sync)
        {
            Subscriptions.Remove(sub);
        }

        Wake.Set();
        Log.Debug($"[{sub.Owner}] unsubscribed");
    }

    private static void EnsureThread()
    {
        if (_thread is { IsAlive: true })
        {
            return;
        }

        if (_thread != null)
        {
            Log.Error("Pump thread was dead — restarting it.");
        }
        else
        {
            TreeRoot.OnStop += _ => Wake.Set();
        }

        _thread = new Thread(PumpLoop)
        {
            IsBackground = true,
            Name = "LlamaLibrary FramePump",
        };
        _thread.Start();
    }

    private static void PumpLoop()
    {
        Log.Information("Pump thread started");
        while (true)
        {
            try
            {
                PumpTick();
            }
            catch (Exception e)
            {
                // The pump thread must never die silently — every subscription and queued
                // one-shot depends on it. Log, back off briefly, keep going.
                Log.Error($"Pump tick failed: {e.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    private static void PumpTick()
    {
        Wake.WaitOne(ComputeWait());

        List<FrameSubscription>? due = null;
        List<OneShotWorkItem>? work = null;
        lock (Sync)
        {
            while (OneShots.Count > 0)
            {
                (work ??= new List<OneShotWorkItem>()).Add(OneShots.Dequeue());
            }

            if (!TreeRoot.IsRunning && Subscriptions.Count > 0)
            {
                var now = DateTime.UtcNow;
                due = Subscriptions.Where(s => s.IsDue(now)).ToList();
                if (due.Count == 0)
                {
                    due = null;
                }
            }
        }

        if (work != null)
        {
            work.RemoveAll(i => i.TryCancelBeforeRun());
            if (work.Count == 0)
            {
                work = null;
            }
        }

        if (due == null && work == null)
        {
            return;
        }

        var published = due == null ? null : new List<KeyValuePair<FrameSubscription, object?>>(due.Count);
        try
        {
            // One shared frame hold per tick for every consumer. Keep readers tiny:
            // copy raw values in here, shape/format after release.
            using (Core.Memory.AcquireFrame(true))
            {
                if (due != null)
                {
                    foreach (var sub in due)
                    {
                        if (sub.TryRead(Log, FaultBackoff, out var value))
                        {
                            published!.Add(new KeyValuePair<FrameSubscription, object?>(sub, value));
                        }
                    }
                }

                if (work != null)
                {
                    foreach (var item in work)
                    {
                        item.Execute();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Frame acquisition failed: {e.Message}");
            work?.ForEach(i => i.Fault(e));

            // Push still-due subscriptions onto the fault backoff so a broken frame hook
            // becomes a slow retry, not a zero-wait hot loop.
            if (due != null)
            {
                var now = DateTime.UtcNow;
                foreach (var sub in due.Where(s => s.IsDue(now)))
                {
                    sub.Defer(FaultBackoff);
                }
            }
        }

        // Outside the frame lock: complete awaiters and post snapshots to UI contexts.
        work?.ForEach(i => i.Complete());
        if (published != null)
        {
            foreach (var kv in published)
            {
                kv.Key.Publish(kv.Value, Log);
            }
        }
    }

    private static TimeSpan ComputeWait()
    {
        lock (Sync)
        {
            if (OneShots.Count > 0)
            {
                return TimeSpan.Zero;
            }

            if (Subscriptions.Count == 0)
            {
                return Timeout.InfiniteTimeSpan;
            }

            // While the bot runs, recurring ticks are paused — but never sleep unbounded on the
            // OnStop wake alone: TreeRoot.OnStop fires milliseconds BEFORE TreeRoot.IsRunning
            // flips false (observed on 1.0.9003.0), so a wake that races the flip would strand
            // every subscription in an infinite sleep. Bounded wait; recurring resumes <=1s
            // after a stop even if the wake is lost.
            if (TreeRoot.IsRunning)
            {
                return TimeSpan.FromSeconds(1);
            }

            var now = DateTime.UtcNow;
            var next = Subscriptions.Select(s => s.NextDue).DefaultIfEmpty(now + DefaultInterval).Min();
            var wait = next - now;
            return wait <= TimeSpan.Zero ? TimeSpan.Zero : wait;
        }
    }
}

/// <summary>
/// A recurring FramePump registration. Dispose on window unload; disposing stops future reads
/// and suppresses any in-flight publish.
/// </summary>
public sealed class FrameSubscription : IDisposable
{
    private readonly Func<object?> _reader;
    private readonly Action<object?> _apply;
    private readonly SynchronizationContext? _context;
    private int _disposed;
    private int _consecutiveFaults;

    internal FrameSubscription(string owner, Func<object?> reader, Action<object?> apply, TimeSpan interval, SynchronizationContext? context)
    {
        Owner = owner;
        _reader = reader;
        _apply = apply;
        Interval = interval;
        _context = context;
        NextDue = DateTime.UtcNow;
    }

    /// <summary>Gets the diagnostic name supplied at registration.</summary>
    public string Owner { get; }

    /// <summary>Gets or sets the tick interval for this subscription.</summary>
    public TimeSpan Interval { get; set; }

    /// <summary>Gets a value indicating whether this subscription has been disposed.</summary>
    public bool IsDisposed => _disposed != 0;

    internal DateTime NextDue { get; private set; }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            FramePump.Remove(this);
        }
    }

    internal bool IsDue(DateTime utcNow)
    {
        return !IsDisposed && utcNow >= NextDue;
    }

    internal void Defer(TimeSpan backoff)
    {
        NextDue = DateTime.UtcNow + backoff;
    }

    internal bool TryRead(LLogger log, TimeSpan faultBackoff, out object? value)
    {
        value = null;
        try
        {
            value = _reader();
            NextDue = DateTime.UtcNow + Interval;
            if (_consecutiveFaults > 0)
            {
                log.Information($"[{Owner}] reader recovered after {_consecutiveFaults} fault(s)");
                _consecutiveFaults = 0;
            }

            return true;
        }
        catch (Exception e)
        {
            _consecutiveFaults++;
            NextDue = DateTime.UtcNow + faultBackoff;
            if (_consecutiveFaults == 1 || _consecutiveFaults % 10 == 0)
            {
                log.Error($"[{Owner}] reader failed (x{_consecutiveFaults}): {e.Message}");
            }

            return false;
        }
    }

    internal void Publish(object? value, LLogger log)
    {
        if (IsDisposed)
        {
            return;
        }

        if (_context != null)
        {
            _context.Post(
                _ =>
                {
                    if (!IsDisposed)
                    {
                        SafeApply(value, log);
                    }
                },
                null);
        }
        else
        {
            SafeApply(value, log);
        }
    }

    private void SafeApply(object? value, LLogger log)
    {
        try
        {
            _apply(value);
        }
        catch (Exception e)
        {
            log.Error($"[{Owner}] apply failed: {e.Message}");
        }
    }
}

internal sealed class OneShotWorkItem
{
    private readonly Func<object?> _work;
    private readonly CancellationToken _ct;
    private readonly TaskCompletionSource<object?> _tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
    private object? _result;
    private Exception? _error;
    private bool _ran;

    internal OneShotWorkItem(Func<object?> work, CancellationToken ct)
    {
        _work = work;
        _ct = ct;
    }

    internal Task<object?> Task => _tcs.Task;

    internal bool TryCancelBeforeRun()
    {
        if (!_ct.IsCancellationRequested)
        {
            return false;
        }

        _tcs.TrySetCanceled(_ct);
        return true;
    }

    internal void Execute()
    {
        try
        {
            _result = _work();
        }
        catch (Exception e)
        {
            _error = e;
        }
        finally
        {
            _ran = true;
        }
    }

    internal void Fault(Exception e)
    {
        if (!_ran)
        {
            _error = e;
            _ran = true;
        }
    }

    internal void Complete()
    {
        if (_error != null)
        {
            _tcs.TrySetException(_error);
        }
        else
        {
            _tcs.TrySetResult(_result);
        }
    }
}
