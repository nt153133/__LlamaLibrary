using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Interfaces;
using ff14bot.Managers;
using ff14bot.NeoProfile;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using TreeSharp;

namespace LlamaLibrary;

public abstract class TemplatePlugin : BotPlugin, IBotPlugin
{
    private static volatile PulseFlags _pulseFlags = PulseFlags.None;
    private static int _pulseDelay = 500;
    private static Thread _pulseThread;
    private static volatile bool _pulseThreadRunning;
    private static int _criticalUpdateCount;
    private static int _requiredPulseFlagsCount;

    private static readonly object PulseLock = new();

    private static bool _threadHooksSet;

    private static readonly List<PulseFlags> PulseFlagOptions = Enum.GetValues(typeof(PulseFlags)).OfType<PulseFlags>().Where(i => i != PulseFlags.None && i != PulseFlags.All).ToList();

    private static ConcurrentDictionary<string, ConcurrentBag<PulseFlags>> PulseFlagBreakdown = new();

    private static readonly string TempListName = $"{nameof(TemplatePlugin)}{Core.Memory.Process.Id}";
    private readonly List<ActionRunCoroutine> _orderBotHooksCoroutines = new();
    private List<Func<Task>> _lisbethHooks = new();

    private List<Func<Task<bool>>> _orderBotHooks = new();
    private int _pulseDelayOld = 500;

    private PulseFlags _pulseFlagsOld = PulseFlags.None;

    private Form? _settings;

    private readonly List<PulseFlags> _tempPulseFlags = new();
    protected LLogger Log = null!;
    public abstract string PluginName { get; }

    protected virtual Color LogColor { get; } = Colors.CornflowerBlue;

    protected virtual Type? SettingsForm { get; } = null;

    protected virtual bool RequiresPulseThread { get; } = false;

    protected virtual PulseFlags PulseFlags { get; } = PulseFlags.None;

    public static int FPS => (int)Core.Memory.NoCacheRead<float>(Core.Memory.Read<IntPtr>(Offsets.Framework) + Offsets.Framerate);
    private static int CriticalPulseDelay => Math.Max(25, 1000 / FPS) * 2;

    public static ConcurrentBag<PulseFlags> BreakDownPulseFlags(PulseFlags flags)
    {
        if (flags == PulseFlags.All)
        {
            return new ConcurrentBag<PulseFlags>(PulseFlagOptions);
        }

        return new ConcurrentBag<PulseFlags>(PulseFlagOptions.Where(r => flags.HasFlag(r)).ToList());
    }

    private static void StartPulseThread()
    {
        lock (PulseLock)
        {
            if (_pulseThreadRunning)
            {
                return;
            }

            _pulseThreadRunning = true;
            _pulseThread = new Thread(PulseThread);
            _pulseThread.Start();
        }
    }

    private static void StopPulseThread()
    {
        lock (PulseLock)
        {
            if (!_pulseThreadRunning)
            {
                return;
            }

            _pulseThreadRunning = false;
            _pulseThread.Join();
        }
    }

    protected virtual LLogger GetLogger()
    {
        return new LLogger(PluginName, LogColor);
    }

    private void AddHooks()
    {
        if (_orderBotHooksCoroutines.Any())
        {
            foreach (var orderBotHook in _orderBotHooksCoroutines)
            {
                //Log.Information($"Adding Orderbot {orderBotHook.Runner.Method.Name} Hook");

                TreeHooks.Instance.AddHook("TreeStart", orderBotHook);
            }
        }

        if (!HasLisbeth())
        {
            return;
        }

        if (!_lisbethHooks.Any())
        {
            return;
        }

        foreach (var lisbethHook in _lisbethHooks)
        {
            var hooks = Lisbeth.GetHookList();
            var baseHook = lisbethHook.Method.Name;
            var craftCycleHook = baseHook + "_Craft";
            if (!hooks.Contains(baseHook))
            {
                Log.Information($"Adding Lisbeth {lisbethHook.Method.Name} Hook");
                Lisbeth.AddHook(baseHook, lisbethHook);
            }

            if (!hooks.Contains(craftCycleHook))
            {
                Log.Information($"Adding {craftCycleHook} Hook");
                Lisbeth.AddCraftCycleHook(craftCycleHook, lisbethHook);
            }
        }
    }

    private void RemoveHooks()
    {
        if (_orderBotHooksCoroutines.Any())
        {
            foreach (var orderBotHook in _orderBotHooksCoroutines)
            {
                //Log.Information($"Removing Orderbot {orderBotHook.Runner.Method.Name} Hook");

                TreeHooks.Instance.RemoveHook("TreeStart", orderBotHook);
            }
        }

        if (!HasLisbeth())
        {
            return;
        }

        if (!_lisbethHooks.Any())
        {
            return;
        }

        foreach (var lisbethHook in _lisbethHooks)
        {
            var hooks = Lisbeth.GetHookList();
            var baseHook = lisbethHook.Method.Name;
            var craftCycleHook = baseHook + "_Craft";
            if (!hooks.Contains(baseHook))
            {
                Log.Information($"Removing Lisbeth {lisbethHook.Method.Name} Hook");
                Lisbeth.RemoveHook(baseHook);
            }

            if (!hooks.Contains(craftCycleHook))
            {
                Log.Information($"Removing {craftCycleHook} Hook");
                Lisbeth.RemoveCraftCycleHook(craftCycleHook);
            }
        }
    }

    protected virtual void OnBotStop(BotBase bot)
    {
        RemoveHooks();
    }

    protected virtual void OnBotStart(BotBase bot)
    {
        AddHooks();
    }

    private static bool HasLisbeth()
    {
        return BotManager.Bots.Any(c => c.EnglishName == "Lisbeth");
    }

    public virtual List<Func<Task<bool>>> GetOrderBotHooks()
    {
        return new List<Func<Task<bool>>>();
    }

    public virtual List<Func<Task>> GetLisbethHooks()
    {
        return new List<Func<Task>>();
    }

    private static void PulseThread()
    {
        ff14bot.Helpers.Logging.Write(Colors.CornflowerBlue, $@"{nameof(TemplatePlugin)}: PulseThread Started with flags {_pulseFlags}");
        while (_pulseThreadRunning)
        {
            try
            {
                if (!TreeRoot.IsRunning)
                {
                    if (!(GameObjectManager.LocalPlayer == null))
                    {
                        Pulsator.Pulse(_pulseFlags);
                    }
                    else
                    {
                        if (_pulseFlags.HasFlag(PulseFlags.Windows))
                        {
                            //ff14bot.Helpers.Logging.Write(Colors.CornflowerBlue, $@"{nameof(TemplatePlugin)}: Pulsing Windows");
                            RaptureAtkUnitManager.Update();
                        }

                        if (_pulseFlags.HasFlag(PulseFlags.Plugins))
                        {
                            //ff14bot.Helpers.Logging.Write(Colors.CornflowerBlue, $@"{nameof(TemplatePlugin)}: Pulsing Plugins");
                            PluginManager.PulseAllPlugins();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
                //ff14bot.Helpers.Logging.WriteException(e);
            }

            Thread.Sleep(_pulseDelay);
        }

        ff14bot.Helpers.Logging.Write(Colors.CornflowerBlue, $@"{nameof(TemplatePlugin)}: PulseThread Stopped");
    }

    public void EnterCriticalMode()
    {
        if (Interlocked.Increment(ref _criticalUpdateCount) > 1)
        {
            return;
        }

        _pulseDelayOld = Interlocked.Exchange(ref _pulseDelay, CriticalPulseDelay);
    }

    public void ExitCriticalMode()
    {
        if (Interlocked.Decrement(ref _criticalUpdateCount) > 0)
        {
            return;
        }

        Interlocked.Exchange(ref _pulseDelay, _pulseDelayOld);
    }

    private static void BotEventsOnOnBotStopRequested(EventArgs eventArgs)
    {
        StartPulseThread();
    }

    private static void BotEventsOnOnBotStartRequested(EventArgs eventArgs)
    {
        StopPulseThread();
    }

    private static void UpdatePulseFlags()
    {
        var list = PulseFlagBreakdown.Values.SelectMany(i => i).Distinct().ToList();

        if (list.Count == 0)
        {
            _pulseFlags = PulseFlags.None;
            return;
        }

        if (list.Contains(PulseFlags.All))
        {
            _pulseFlags = PulseFlags.All;
            return;
        }

        _pulseFlags = list.Aggregate((a, b) => a | b);
        //ff14bot.Helpers.Logging.Write(Colors.Yellow, $@"{nameof(TemplatePlugin)}: pulseFlags changed to: {_pulseFlags}");
    }

    public void AddTemporaryPulseFlag(PulseFlags flags)
    {
        var list = PulseFlagBreakdown.GetOrAdd(TempListName, new ConcurrentBag<PulseFlags>());
        var newFlags = BreakDownPulseFlags(flags);
        foreach (var newFlag in newFlags)
        {
            if (_tempPulseFlags.Contains(newFlag))
            {
                continue;
            }

            Log.Information($"Adding Temporary PulseFlag: {flags}");
            list.Add(newFlag);
            _tempPulseFlags.Add(newFlag);
        }

        lock (PulseLock)
        {
            UpdatePulseFlags();
        }
    }

    public void RemoveTemporaryPulseFlag(PulseFlags flags)
    {
        if (!_tempPulseFlags.Contains(flags))
        {
            return;
        }

        Log.Information($"Removing Temporary PulseFlag: {flags}");
        if (PulseFlagBreakdown.TryGetValue(TempListName, out var list))
        {
            var newFlags = BreakDownPulseFlags(flags);
            foreach (var newFlag in newFlags)
            {
                list.TryTake(out _);
                _tempPulseFlags.Remove(newFlag);
            }

            lock (PulseLock)
            {
                UpdatePulseFlags();
            }
        }
    }

    public override string Name => PluginName;

    public override void OnInitialize()
    {
        Log = GetLogger();
        Log.Information("Initializing");
        _orderBotHooks = GetOrderBotHooks();
        _lisbethHooks = GetLisbethHooks();

        if (_orderBotHooks.Any())
        {
            foreach (var orderBotHook in _orderBotHooks)
            {
                _orderBotHooksCoroutines.Add(new ActionRunCoroutine(_ => orderBotHook()));
            }
        }
    }

    public override void OnEnabled()
    {
        TreeRoot.OnStart += OnBotStart;
        TreeRoot.OnStop += OnBotStop;
        if (RequiresPulseThread)
        {
            PulseFlagBreakdown.TryAdd(PluginName, BreakDownPulseFlags(PulseFlags));
            foreach (var tempPulseFlag in _tempPulseFlags.ToList())
            {
                AddTemporaryPulseFlag(tempPulseFlag);
            }

            lock (PulseLock)
            {
                UpdatePulseFlags();
                if (Interlocked.Increment(ref _requiredPulseFlagsCount) == 1)
                {
                    BotEvents.OnBotStarted += BotEventsOnOnBotStartRequested;
                    BotEvents.OnBotStopped += BotEventsOnOnBotStopRequested;
                    StartPulseThread();
                    Log.Information("Setting start/stop pulse hooks");
                }
            }
        }

        Log.Information($"{PluginName} Enabled");
    }

    public override void OnDisabled()
    {
        TreeRoot.OnStart -= OnBotStart;
        TreeRoot.OnStop -= OnBotStop;
        if (RequiresPulseThread)
        {
            PulseFlagBreakdown.TryRemove(PluginName, out _);
            foreach (var tempPulseFlag in _tempPulseFlags.ToList())
            {
                RemoveTemporaryPulseFlag(tempPulseFlag);
            }

            lock (PulseLock)
            {
                UpdatePulseFlags();
                if (Interlocked.Decrement(ref _requiredPulseFlagsCount) == 0)
                {
                    BotEvents.OnBotStarted -= BotEventsOnOnBotStartRequested;
                    BotEvents.OnBotStopped -= BotEventsOnOnBotStopRequested;
                    StopPulseThread();
                    Log.Information("Removing start/stop pulse hooks");
                }
            }
        }

        Log.Information($"{PluginName} Disabled");
    }

    public override void OnButtonPress()
    {
        //Log.Debug("ButtonPress");
        if (SettingsForm == null)
        {
            Log.Error("No setting form type was set in the botbase");
            return;
        }

        if (_settings == null || _settings.IsDisposed)
        {
            _settings = Activator.CreateInstance(SettingsForm) as Form;
        }

        try
        {
            if (_settings == null)
            {
                Log.Error($"Could not create settings form object {SettingsForm.Name}");
                return;
            }

            _settings.Show();
            _settings.Activate();
        }
        catch (Exception ee)
        {
            Log.Error($"Exception: {ee.Message}");
        }
    }
}