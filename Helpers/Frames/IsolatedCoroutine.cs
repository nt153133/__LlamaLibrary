using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers.Frames;

/// <summary>
/// Runs a coroutine to completion on a dedicated worker thread while the bot is stopped,
/// without starting TreeRoot — no third-party plugin sees OnStart/OnStop and no hooks churn.
/// Each Resume() executes inside its own hard frame lock at ~30 ticks/sec, mirroring the bot
/// thread's cadence, and every Resume() happens on the same thread for the coroutine's lifetime.
/// <para>
/// Operation rules: only coroutine-native awaits inside (Coroutine.Yield/Sleep/Wait, helpers
/// built on them; bridge TPL tasks with Coroutine.ExternalTask). Keep operations away from
/// Navigator/Flightor and GameEvents-dependent helpers — they assume the real bot context.
/// Cancellation abandons the operation at its current await, so operations must be re-entrant.
/// </para>
/// </summary>
public static class IsolatedCoroutine
{
    private static readonly LLogger Log = new LLogger("IsolatedCoroutine", Colors.MediumPurple);
    private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);

    /// <summary>Gets a value indicating whether an isolated coroutine is currently executing.</summary>
    public static bool IsActive => Gate.CurrentCount == 0;

    /// <summary>
    /// Runs <paramref name="producer"/> as an isolated coroutine and returns its result.
    /// </summary>
    public static async Task<T> RunAsync<T>(Func<Task<T>> producer, CancellationToken ct = default, TimeSpan? tickInterval = null)
    {
        if (producer == null)
        {
            throw new ArgumentNullException(nameof(producer));
        }

        var result = default(T)!;
        await RunAsync(
            async () =>
            {
                result = await producer();
            },
            ct,
            tickInterval).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Runs <paramref name="producer"/> as an isolated coroutine. Throws if the bot is running,
    /// if called from inside a coroutine (await the operation directly there instead), or if
    /// another isolated coroutine is already active — one at a time, by design.
    /// </summary>
    public static async Task RunAsync(Func<Task> producer, CancellationToken ct = default, TimeSpan? tickInterval = null)
    {
        if (producer == null)
        {
            throw new ArgumentNullException(nameof(producer));
        }

        if (Coroutine.Current != null)
        {
            throw new InvalidOperationException("Already inside a coroutine — await the operation directly instead of using IsolatedCoroutine.");
        }

        if (!Gate.Wait(0))
        {
            throw new InvalidOperationException("Another isolated coroutine is already active.");
        }

        try
        {
            if (TreeRoot.IsRunning)
            {
                throw new InvalidOperationException("Bot is running — run the operation from a tree hook or OnPulse instead.");
            }

            var interval = (int)(tickInterval?.TotalMilliseconds ?? 33);
            await Task.Factory.StartNew(
                () => Pump(producer, interval, ct),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).ConfigureAwait(false);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static void Pump(Func<Task> producer, int interval, CancellationToken ct)
    {
        using var coro = new Coroutine(producer);
        while (!coro.IsFinished)
        {
            ct.ThrowIfCancellationRequested();

            // Mirrors the frame-locked tick the bot thread provides. Same-thread nested frame
            // locks (e.g. Lua.* inside this scope) are verified reentrant on 1.0.9003.0 via the
            // FramePumpTest probe; if a future build hangs here, nesting is the first suspect.
            // The IsRunning check sits inside the lock: a bot start between check and resume
            // can no longer interleave with this tick, because the hard lock serializes us
            // against the bot thread's pulses.
            using (Core.Memory.AcquireFrame(true))
            {
                if (TreeRoot.IsRunning)
                {
                    throw new InvalidOperationException("TreeRoot started mid-operation; isolated coroutine aborted.");
                }

                coro.Resume();
            }

            Thread.Sleep(interval);
        }

        if (coro.Status == CoroutineStatus.Faulted && coro.FaultingException != null)
        {
            Log.Error($"Isolated coroutine faulted: {coro.FaultingException.Message}");
            ExceptionDispatchInfo.Capture(coro.FaultingException).Throw();
        }
    }
}
