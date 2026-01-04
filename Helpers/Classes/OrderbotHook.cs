using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using TreeSharp;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Base decorator used to create and register orderbot hooks with the TreeHooks system.
/// </summary>
/// <remarks>
/// Derive from this class and implement <see cref="HookName"/>, <see cref="HookDescription"/>,
/// <see cref="ShouldRun(object)"/> and <see cref="Run"/> to provide hook behavior. The constructor
/// creates a coroutine child that invokes the hook logic and attaches it as the decorator's child.
/// </remarks>
public abstract class OrderbotHook : Decorator
{
    /// <summary>
    /// Logger instance used by the hook for informational and diagnostic messages.
    /// </summary>
    protected readonly LLogger Log;

    private bool _added;
    private bool _includeBusyCheck;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderbotHook"/> class.
    /// </summary>
    /// <param name="logLevel">
    /// The log level used to construct the <see cref="LLogger"/> for this hook.
    /// Defaults to <see cref="LogLevel.Information"/>.
    /// </param>
    /// <param name="includeBusyCheck">
    /// If <c>true</c>, the default busy check is applied in <see cref="CanRun(object)"/> to prevent running
    /// while moving, in combat, dead, in instance, or within a fate. Defaults to <c>true</c>.
    /// </param>
    /// <remarks>
    /// The constructor constructs the logger, creates an <see cref="ActionRunCoroutine"/> child that invokes the hook logic,
    /// assigns it as a child of this decorator, and stores the busy-check preference.
    /// </remarks>
    protected OrderbotHook(LogLevel logLevel = LogLevel.Information, bool includeBusyCheck = true)
    {
        Log = new LLogger(HookName, Colors.DarkOrange, logLevel);
        Composite func = new ActionRunCoroutine(async result => await HookRun());
        func.Parent = this;
        Children = new List<Composite> { func };
        _includeBusyCheck = includeBusyCheck;
    }

    /// <summary>
    /// Gets the hook's display name.
    /// </summary>
    /// <value>The name used for logging, diagnostics and identification of the hook.</value>
    protected abstract string HookName { get; }

    /// <summary>
    /// Gets a short description of the hook's purpose.
    /// </summary>
    /// <value>A human-readable description shown in logs and diagnostics.</value>
    protected abstract string HookDescription { get; }

    /// <summary>
    /// Gets the location key within <c>TreeHooks</c> where this hook will be registered.
    /// </summary>
    /// <value>The hook registration location; defaults to <c>TreeStart</c>.</value>
    /// <remarks>
    /// Override to change where the hook is attached (for example, <c>TreeStart</c>, <c>DeathReturnLogic</c> ,<c>DeathReviveLogic</c> ,<c>PoiAction</c> ,<c>Pull</c> ,<c>RoutineCombat</c> ,<c>HotspotPoi</c> ,<c>PoiAction2</c> ,<c>SetDeathPoi</c> ,<c>SetCombatPoi</c> ,<c>SetHotspotPoi</c> ,<c>SelectPoiType</c> ).
    /// </remarks>
    protected virtual string Location => "TreeStart";

    /// <summary>
    /// Determines whether this hook should run for the provided behavior-tree <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The behavior-tree context passed into <see cref="CanRun(object)"/>.</param>
    /// <returns><c>true</c> if the hook should run for the given context; otherwise <c>false</c>.</returns>
    /// <remarks>Implementations should be quick and non-blocking. This is used by the decorator to decide execution.</remarks>
    protected abstract bool ShouldRun(object context);

    /// <summary>
    /// Executes the hook's logic asynchronously.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that completes with <c>true</c> if the hook completed successfully
    /// (and any downstream behavior should consider the run successful); otherwise <c>false</c>.
    /// </returns>
    /// <remarks>This method is invoked when <see cref="ShouldRun(object)"/> returns <c>true</c>. Implementations may perform async work.</remarks>
    protected abstract Task<bool> Run();

    /// <summary>
    /// Determines whether the decorator can run in the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The behavior-tree context.</param>
    /// <returns><c>false</c> if the default busy check disallows running; otherwise returns the result of <see cref="ShouldRun(object)"/>.</returns>
    protected override bool CanRun(object context)
    {
        if (_includeBusyCheck && DefaultBusyCheck())
        {
            return false;
        }

        return ShouldRun(context);
    }

    /// <summary>
    /// Internal wrapper that times and logs execution of <see cref="Run"/>.
    /// </summary>
    /// <returns>The boolean result returned by <see cref="Run"/>.</returns>
    private async Task<bool> HookRun()
    {
        var timer = Stopwatch.StartNew();
        Log.Information($"{HookName} started");
        var result = await Run();
        timer.Stop();
        Log.Information($"{HookName} took {timer.Elapsed} to complete");
        return result;
    }

    /// <summary>
    /// Adds this hook to the <c>TreeHooks</c> collection at <see cref="Location"/>, if not already added.
    /// </summary>
    public virtual void AddHook()
    {
        if (_added)
        {
            return;
        }

        if (TreeHooks.Instance.Hooks.TryGetValue(Location, out var list) && list.Any(Equals))
        {
            return;
        }

        TreeHooks.Instance.AddHook(Location, this);
        Log.Information($"{Location} hook added ({Guid})");
        TreeHooks.Instance.OnHooksCleared -= OnHooksCleared;
        TreeHooks.Instance.OnHooksCleared += OnHooksCleared;
        _added = true;
    }

    /// <summary>
    /// Removes this hook from the <c>TreeHooks</c> collection at <see cref="Location"/> if it was added.
    /// </summary>
    public virtual void RemoveHook()
    {
        if (!_added)
        {
            return;
        }

        TreeHooks.Instance.RemoveHook(Location, this);
        Log.Information($"{Location} hook Removed ({Guid})");
        TreeHooks.Instance.OnHooksCleared -= OnHooksCleared;
        _added = false;
    }

    /// <summary>
    /// Returns the hook display name and Guid.
    /// </summary>
    /// <returns>The value of <see cref="HookName"/>.</returns>
    public override string ToString()
    {
        return $"{HookName} {Guid}";
    }

    /// <summary>
    /// Handler called when <see cref="TreeHooks.Instance"/> raises <c>OnHooksCleared</c>.
    /// The hook will attempt to re-add itself after hooks are cleared.
    /// </summary>
    private void OnHooksCleared(object? sender, EventArgs args)
    {
        _added = false;
        Log.Information($"{Location} hook Removed ({Guid}) on HooksCleared");
        AddHook();
    }

    /// <summary>
    /// Determines equality by comparing the <see cref="Composite.Guid"/> values of the objects.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the objects have the same <c>Guid</c>; otherwise <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null                => false,
            OrderbotHook other  => Guid == other.Guid,
            Composite composite => Guid == composite.Guid,
            _                   => false
        };
    }

    /// <summary>
    /// Returns the hash code for this instance, derived from the underlying <c>Guid</c>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }

    /// <summary>
    /// Performs the default busy check used to prevent hooks from running when the player is unavailable.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the player is moving, occupied, in combat, dead, inside an instance, or within a fate; otherwise <c>false</c>.
    /// </returns>
    public static bool DefaultBusyCheck()
    {
        return MovementManager.IsMoving || MovementManager.IsOccupied || Core.Me.InCombat || !Core.Me.IsAlive || DutyManager.InInstance || FateManager.WithinFate;
    }
}
