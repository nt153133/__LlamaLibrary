/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Hooks;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory.PatternFinders;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.Settings;
using Newtonsoft.Json;
using LogLevel = LlamaLibrary.Logging.LogLevel;
using PatchManager = LlamaLibrary.Hooks.PatchManager;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InterpolatedStringExpressionIsNotIFormattable

namespace LlamaLibrary.Memory;

public static class OffsetManager
{
    private const long _version = 48;
    private const bool _debug = false;

    // --- Namespace / type filters ---------------------------------------------------------------
    private const string MemoryNamespacePrefix = "LlamaLibrary.Memory";
    private const string OffsetsClassToken = "Offsets";
    private const string RemoteAgentsNamespace = "LlamaLibrary.RemoteAgents";

    // --- State ----------------------------------------------------------------------------------
    public static readonly Dictionary<string, string> patterns = new(StringComparer.Ordinal);
    public static readonly Dictionary<string, string> constants = new(StringComparer.Ordinal);
    public static ConcurrentDictionary<string, long> OffsetCache = new(StringComparer.Ordinal);

    public static LLogger Logger { get; } = new("LLOffsetManager", Colors.RosyBrown, LogLevel.Debug);

    private static readonly TaskCompletionSource<bool> InitTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static bool initDone;
    private static bool _isNewGameBuild;

    private static int _gameVersionCache;
    private static int GameVersion
    {
        get
        {
            if (_gameVersionCache != 0) return _gameVersionCache;
            try { return _gameVersionCache = Core.CurrentGameVer; }
            catch { return 0; }
        }
    }

    private static string OffsetFile { get; } = Path.Combine(JsonSettings.SettingsPath, $"LL_Offsets_{GameVersion}.json");

    // --- Region / record ------------------------------------------------------------------------
    public static readonly GameRecord ActiveRecord;
    public static readonly ClientRegion ActiveRegion;

    [Obsolete("Use ActiveRegion instead")]
    public static readonly bool IsChinese;

    [Obsolete("Use ActiveRecord instead")]
    public static readonly float CurrentGameVersion;

    static OffsetManager()
    {
        ActiveRegion = DataManager.CurrentLanguage switch
        {
            Language.Chn                  => ClientRegion.China,
            Language.MainlandTraditional  => ClientRegion.China,
            Language.Korean               => ClientRegion.Korea,
            Language.TraditionalChinese   => ClientRegion.TraditionalChinese,
            _                             => ClientRegion.Global,
        };

        ActiveRecord = ActiveRegion switch
        {
            ClientRegion.China              => new GameRecord(7.45f, OffsetFlags.China),
            ClientRegion.Korea              => new GameRecord(7.3f,  OffsetFlags.Korea),
            ClientRegion.TraditionalChinese => new GameRecord(7.1f,  OffsetFlags.TraditionalChinese),
            _                               => new GameRecord(7.45f, OffsetFlags.Global),
        };

        IsChinese = ActiveRegion == ClientRegion.China;
        CurrentGameVersion = ActiveRecord.CurrentGameVersion;
    }

    // --- Init -----------------------------------------------------------------------------------
    [Obsolete]
    public static void Init() { }

    public static async Task<bool> InitLib()
    {
        var total = Stopwatch.StartNew();
        try
        {
            var sw = Stopwatch.StartNew();
            await SearchAndSetLL().ConfigureAwait(false);
            sw.Stop();
            Logger.Debug($"OffsetManager SearchAndSetLL took {sw.ElapsedMilliseconds}ms");

            Logger.Information($"OffsetManager Init took {total.ElapsedMilliseconds}ms {CallerName()}");
            PrintLastCommit();
            Logger.Information($"Dalamud Dectected: {GeneralFunctions.DalamudDetected()}");
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
        finally
        {
            initDone = true;
            InitTcs.TrySetResult(true);
            total.Stop();
            Logger.Debug($"OffsetManager Init took {total.ElapsedMilliseconds}ms");
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string CallerName()
    {
        // Skip 2 frames: CallerName and its direct caller's caller.
        var frame = new StackTrace().GetFrame(2);
        return frame?.GetMethod()?.DeclaringType?.Name ?? "<unknown>";
    }

    private static void PrintLastCommit()
    {
        var path = Path.Combine(GeneralFunctions.SourceDirectory()?.Parent?.FullName ?? string.Empty, "LastCommit.txt");
        if (!File.Exists(path)) return;

        var raw = File.ReadAllText(path).Trim();
        if (DateTime.TryParse(raw, out var parsed))
        {
            Logger.Information($"Last Commit: {parsed.ToUniversalTime():ddd, dd MMM yyy HH:mm:ss ‘UTC’}");
            Logger.Information($"Raw Last Commit: {raw}");
        }
        else
        {
            Logger.Information($"Last Commit: '{raw}'");
        }
    }

    private static async Task SearchAndSetLL()
    {
        if (File.Exists(OffsetFile) && GameVersion != 0)
        {
            try
            {
                var json = await File.ReadAllTextAsync(OffsetFile).ConfigureAwait(false);
                OffsetCache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, long>>(json)
                              ?? new ConcurrentDictionary<string, long>(StringComparer.Ordinal);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed reading offset cache, starting fresh: {e.Message}");
                OffsetCache = new ConcurrentDictionary<string, long>(StringComparer.Ordinal);
            }

            if (!OffsetCache.TryGetValue("Version", out var v) || v != _version)
            {
                OffsetCache.Clear();
                OffsetCache["Version"] = _version;
                _isNewGameBuild = true;
            }
        }
        else
        {
            OffsetCache = new ConcurrentDictionary<string, long>(StringComparer.Ordinal)
            {
                ["Version"] = _version
            };
             _isNewGameBuild = true;
        }

        await SetOffsetObjectsAsync(GetOffsetTypes()).ConfigureAwait(false);
    }

    // --- Cache management -----------------------------------------------------------------------
    internal static void ClearOffsetFromCache(MemberInfo? info)
    {
        if (info == null)
        {
            Logger.Error("MemberInfo is null");
            return;
        }
        ClearOffsetFromCache(info.MemberName());
    }

    internal static void ClearOffsetFromCache(string name)
    {
        OffsetCache.TryRemove(name, out _);
        if (GameVersion != 0) ScheduleCacheWrite();
    }

    // --- Type discovery -------------------------------------------------------------------------
    private static List<Type>? _cachedTypes;

    private static List<Type> GetOffsetTypes() =>
        _cachedTypes ??= BuildOffsetTypes(Assembly.GetExecutingAssembly(), requireMemoryNamespace: true);

    private static List<Type> GetTypes(Assembly assembly) =>
        BuildOffsetTypes(assembly, requireMemoryNamespace: false);

    private static List<Type> BuildOffsetTypes(Assembly assembly, bool requireMemoryNamespace)
    {
        var result = new List<Type>();
        foreach (var t in assembly.GetTypes())
        {
            if (!t.IsClass || t.Namespace == null) continue;
            if (requireMemoryNamespace && !t.Namespace.StartsWith(MemoryNamespacePrefix, StringComparison.Ordinal)) continue;
            if (!t.Name.Contains(OffsetsClassToken, StringComparison.Ordinal)) continue;
            result.Add(t);
        }

        if (!result.Contains(typeof(Offsets)))
            result.Add(typeof(Offsets));

        return result;
    }

    // Build a IntPtr -> index lookup for agent vtables.
    private static Dictionary<IntPtr, int> BuildVTableLookup()
    {
        var ptrs = AgentModule.AgentVtables;
        var map = new Dictionary<IntPtr, int>(ptrs.Count);
        for (var i = 0; i < ptrs.Count; i++)
        {
            // TryAdd keeps the first index if duplicates exist (matches original behavior)
            map.TryAdd(ptrs[i], i);
        }
        return map;
    }

    // --- Post-offset agent wiring --------------------------------------------------------------
    internal static void SetPostOffsets()
    {
        var sw = Stopwatch.StartNew();
        var vtables = BuildVTableLookup();
        Logger.Debug($"OffsetManager AgentModule.AgentVtables took {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var agentTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, Namespace: RemoteAgentsNamespace } && typeof(IAgent).IsAssignableFrom(t))
            .ToList();
        Logger.Debug($"OffsetManager GetTypesAgents took {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var added = RegisterAgentTypes(agentTypes, vtables);
        Logger.Information($"Added {added} agents");
        Logger.Debug($"OffsetManager AgentModule.TryAddAgent took {sw.ElapsedMilliseconds}ms");

        // Inventory patch bookkeeping
        HandleInventoryPatchState(out var skipInventoryPatch);

        if (GameVersion != 0) ScheduleCacheWrite();

        PatchManager.Initialize(skipInventoryPatch);
    }

    private static int RegisterAgentTypes(IEnumerable<Type> agentTypes, IReadOnlyDictionary<IntPtr, int> vtables)
    {
        var objects = new object[] { IntPtr.Zero };
        var added = 0;
        foreach (var t in agentTypes)
        {
            var agent = (IAgent)Activator.CreateInstance(
                t,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                objects,
                null)!;

            if (vtables.TryGetValue(agent.RegisteredVtable, out var idx))
            {
                var found = AgentModule.TryAddAgent(idx, t);
                if (found)
                {
                    added++;
                }
                else
                {
                    Logger.Error($"\tFailed to add Agent {t.Name}");
                }

            }
            else
            {
                Logger.Error($"\tFound one {t.Name} {agent.RegisteredVtable:X} ({Core.Memory.GetRelative(agent.RegisteredVtable):X}) but no agent");
            }
        }
        return added;
    }

    private static void HandleInventoryPatchState(out bool skipInventoryPatch)
    {
        var settings = LlamaLibrarySettings.Instance;

        if (settings.TempDisableInventoryHook &&
            InventoryUpdatePatchOffsets.OrginalCall == InventoryUpdatePatchOffsets.OriginalJump)
        {
            settings.TempDisableInventoryHook = false;
        }

        if (GameVersion != settings.LastRevision &&
            InventoryUpdatePatchOffsets.OrginalCall == InventoryUpdatePatchOffsets.OriginalJump)
        {
            settings.LastRevision = GameVersion;
            Logger.Information($"Setting revision to {GameVersion} in {settings.FilePath}");
        }

        skipInventoryPatch = settings.TempDisableInventoryHook || settings.DisableInventoryHook || LibraryClass.SafeMode;

        Logger.Information($"TempDisableInventoryHook: {settings.TempDisableInventoryHook} DisableInventoryHook: {settings.DisableInventoryHook}");

        if (skipInventoryPatch) return;

        var origCall = InventoryUpdatePatchOffsets.OrginalCall;
        var origJump = InventoryUpdatePatchOffsets.OriginalJump;

        if (origCall == origJump && origCall != IntPtr.Zero && origJump != IntPtr.Zero)
        {
            return; // already consistent
        }

        if (!_isNewGameBuild && origCall != IntPtr.Zero && origJump != IntPtr.Zero)
        {
            Logger.Information("Last patch not cleaned up, cleaning up now");
            var asm = Core.Memory.Asm;
            asm.Clear();
            asm.AddLine("[org 0x{0:X16}]", (ulong)InventoryUpdatePatchOffsets.PatchLocation);
            asm.AddLine("JMP {0}", origCall);
            Core.Memory.WriteBytes(InventoryUpdatePatchOffsets.PatchLocation, asm.Assemble());
            InventoryUpdatePatchOffsets.OriginalJump = origCall;
        }
        else
        {
            Logger.Error("New game build and inventory patch offsets don't match");
            var memberInfo = typeof(InventoryUpdatePatchOffsets)
                .GetMember("OrginalCall", BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault();
            ClearOffsetFromCache(memberInfo);
            settings.TempDisableInventoryHook = true;
            skipInventoryPatch = true;
        }
    }

    public static void RegisterAgent(IAgent iagent)
    {
        var vtables = BuildVTableLookup();
        var type = iagent.GetType();

        if (vtables.TryGetValue(iagent.RegisteredVtable, out var idx))
        {
            Logger.Information($"\tTrying to add {type} {AgentModule.TryAddAgent(idx, type)}");
        }
        else
        {
            Logger.Error($"\tFound one {type.Name} {iagent.RegisteredVtable:X} ({Core.Memory.GetRelative(iagent.RegisteredVtable):X}) but no agent");
        }
    }

    // --- Script manager helpers ----------------------------------------------------------------
    internal static void AddNamespacesToScriptManager(params string[] param)
    {
        var field = typeof(ScriptManager)
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .FirstOrDefault(f => f.FieldType == typeof(List<string>));
        if (field == null) return;

        try
        {
            if (field.GetValue(null) is not List<string> list) return;

            foreach (var ns in param)
            {
                if (list.Contains(ns, StringComparer.Ordinal)) continue;
                list.Add(ns);
                Logger.Information($"Added namespace '{ns}' to ScriptManager");
            }
        }
        catch
        {
            Logger.Error("Failed to add namespaces to ScriptManager, this can cause issues with some profiles.");
        }
    }

    // --- Member discovery ----------------------------------------------------------------------
    private const BindingFlags OffsetMemberFlags =
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

    public static IEnumerable<MemberInfo> MemberInfos(Type j)
    {
        foreach (var f in j.GetFields(OffsetMemberFlags))
            if (!f.IsInitOnly && !f.IsPrivate)
                yield return f;

        foreach (var p in j.GetProperties(OffsetMemberFlags))
            if (p.CanWrite)
                yield return p;
    }

    public static IEnumerable<MemberInfo> MemberInfos(IEnumerable<Type> j) => j.SelectMany(MemberInfos);

    // --- Offset search --------------------------------------------------------------------------
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static async Task SetOffsetObjectsAsync(IEnumerable<Type> q1)
    {
        var allMembers = MemberInfos(q1).ToList();
        var missing = ResolveFromCacheAndCollectMissing(allMembers);

        if (missing.Count == 0)
        {
            Logger.Information("All offsets found in cache");
            return;
        }

        Logger.Information($"Not all ({missing.Count}) offsets found in cache");

        var pf = PatternFinderProxy.PatternFinder;
        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        await Parallel.ForEachAsync(missing, options, (member, _) =>
        {
            SearchOffset(member, pf);
            return ValueTask.CompletedTask;
        }).ConfigureAwait(false);
    }

    public static void SetOffsetObjects(IEnumerable<Type> q1)
    {
        var allMembers = MemberInfos(q1).ToList();
        var missing = ResolveFromCacheAndCollectMissing(allMembers);

        if (missing.Count == 0)
        {
            Logger.Information("All offsets found in cache");
            return;
        }

        var pf = PatternFinderProxy.PatternFinder;
        Parallel.ForEach(missing, member => SearchOffset(member, pf));
    }

    /// <summary>
    /// Applies cached offsets to fields where present and returns the list of members
    /// still needing a pattern search.
    /// </summary>
    private static List<MemberInfo> ResolveFromCacheAndCollectMissing(List<MemberInfo> members)
    {
        var missing = new List<MemberInfo>(members.Count);
        foreach (var member in members)
        {
            var type = member.GetMemberType();
            if (type == null)
            {
                Logger.Error($"Parsing class {member.Name}, this shouldn't happen");
                continue;
            }

            if (!member.IgnoreCache() && OffsetCache.TryGetValue(member.MemberName(), out var cached))
            {
                member.SetValue(cached);
                continue;
            }

            missing.Add(member);
        }
        return missing;
    }

    private static void SearchOffset(MemberInfo info, ISearcher pf)
    {
        var type = info.GetMemberType();

        if (type is { IsClass: true, IsAbstract: false })
        {
            SearchOffsetForClass(info, type, pf);
            return;
        }

        var result = info.GetOffset(pf);
        if (result == IntPtr.Zero)
        {
            LogNotFound(info);
            Logger.Error($"{info.GetPattern()}");
        }

        try
        {
            info.SetValue(result);
        }
        catch (Exception e)
        {
            Logger.Error($"Error on {info.GetMemberType()} {result.ToInt64():X}");
            Logger.Exception(e);
        }
    }

    private static void SearchOffsetForClass(MemberInfo info, Type type, ISearcher pf)
    {
        Logger.Information("Trying to set " + type.Name + " (Class)");
        var instance = Activator.CreateInstance(type);
        if (instance == null)
        {
            Logger.Error($"Failed to create instance of {type.Name}");
            return;
        }

        foreach (var field in MemberInfos(type))
        {
            Logger.Information("Trying to set " + field.Name);
            var result = field.GetOffset(pf);

            if (result == IntPtr.Zero)
            {
                LogNotFound(field);
                Logger.Error($"{field.GetPattern()}");
                continue;
            }

            field.SetValue(instance, result);
            Logger.Information($"Setting {result:X} to {instance.DynamicString()}");
        }

        info.SetValue(null, instance);
    }

    private static void LogNotFound(MemberInfo field)
    {
        var declaring = field.DeclaringType;
        var ownerName = declaring != null && declaring.IsNested
            ? declaring.DeclaringType?.Name
            : declaring?.Name;
        Logger.Error($"[{ownerName}:{field.Name:,27}] Not Found");
    }

    internal static void SetScriptsThread()
    {
        Logger.Information("Setting ScriptManager");
        Task.Run(() =>
        {
            ScriptManager.AddNamespaces("LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras");
            ScriptManager.Init(typeof(ScriptConditions.Helpers));
            Logger.Information("ScriptManager Set");
        });
    }

    // --- Namespace helpers ----------------------------------------------------------------------
    public static string? GetRootNamespace(string? nameSpace)
    {
        if (nameSpace == null) return null;
        var idx = nameSpace.IndexOf('.', StringComparison.Ordinal);
        return idx > 0 ? nameSpace.Substring(0, idx) : nameSpace;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string? GetCurrentNamespace()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;
        return GetRootNamespace(type?.Namespace);
    }

    /// <summary>
    /// Collects the nested "Offsets" classes from every class in the caller's root namespace.
    /// </summary>
    private static List<Type> CollectOffsetClassesFromCaller(MethodBase? callerMethod)
    {
        var declaringType = callerMethod?.DeclaringType;
        var assembly = declaringType?.Assembly;
        var root = GetRootNamespace(declaringType?.Namespace);
        if (assembly == null || root == null) return new List<Type>();

        var result = new List<Type>();
        foreach (var t in assembly.GetTypes())
        {
            if (!t.IsClass || t.Namespace == null) continue;
            if (!t.Namespace.Contains(root, StringComparison.Ordinal)) continue;

            var nested = t.GetNestedType(OffsetsClassToken, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            if (nested != null) result.Add(nested);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<Type> GetOffsetClasses()
    {
        var caller = new StackFrame(1).GetMethod();
        return CollectOffsetClassesFromCaller(caller);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetOffsetClasses()
    {
        var caller = new StackFrame(1).GetMethod();
        var q1 = CollectOffsetClassesFromCaller(caller);
        SetOffsetObjects(q1);
        InitTcs.Task.GetAwaiter().GetResult();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetOffsetClassesAndAgents()
    {
        var caller = new StackFrame(1).GetMethod();
        var q1 = CollectOffsetClassesFromCaller(caller);
        SetOffsetObjects(q1);

        if (GameVersion != 0) ScheduleCacheWrite();

        var vtables = BuildVTableLookup();
        var assembly = caller?.DeclaringType?.Assembly;
        if (assembly == null)
        {
            InitTcs.Task.GetAwaiter().GetResult();
            return;
        }

        var agentTypes = assembly.GetTypes().Where(t => t.IsClass && typeof(IAgent).IsAssignableFrom(t));
        var objects = new object[] { IntPtr.Zero };

        foreach (var MyType in agentTypes)
        {
            var agent = (IAgent)Activator.CreateInstance(
                MyType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                objects,
                null)!;

            if (vtables.TryGetValue(agent.RegisteredVtable, out var idx))
                Logger.WriteLog(Colors.BlueViolet, $"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(idx, MyType)}");
            else
                Logger.WriteLog(Colors.BlueViolet, $"\tFound one {MyType.Name} {agent.RegisteredVtable:X} but no agent");
        }

        InitTcs.Task.GetAwaiter().GetResult();
    }

    // --- Pattern dictionary dump ----------------------------------------------------------------
    public static Dictionary<string, string> LLDict(ClientRegion mode = ClientRegion.Global) =>
        BuildPatternDictionary(GetOffsetTypes(), mode);

    public static Dictionary<string, string> LLDict(Assembly assembly, ClientRegion mode = ClientRegion.Global) =>
        BuildPatternDictionary(GetTypes(assembly), mode);

    private static Dictionary<string, string> BuildPatternDictionary(List<Type> sourceTypes, ClientRegion mode)
    {
        var results = new Dictionary<string, string>(StringComparer.Ordinal);
        var types = MemberInfos(sourceTypes).ToList();
        Logger.Information($"Count: {types.Count}");

        foreach (var field in types)
        {
            var declaring = field.DeclaringType;
            if (declaring == null) continue;

            var ownerName = declaring.IsNested ? declaring.DeclaringType?.Name : declaring.Name;
            if (declaring.IsNested && declaring.DeclaringType == null)
            {
                // mirror old "skip deeply-nested without enclosing declaring type" behavior
                continue;
            }

            var offset = field.GetAttribute(mode);
            if (offset == null)
            {
                Logger.Information($"{ownerName}_{field.Name} has no OffsetAttribute!");
                continue;
            }

            var key = $"{ownerName}_{field.Name}";
            try
            {
                results.Add(key, offset.Pattern);
            }
            catch (Exception e)
            {
                Logger.Information($"\t{key} DUPE/Issue");
                Console.WriteLine(e);
            }
        }

        return results;
    }

    // --- Cache debounce write -------------------------------------------------------------------
    private static CancellationTokenSource? _writeCts;
    private static readonly SemaphoreSlim WriteLock = new(1, 1);

    private static void ScheduleCacheWrite()
    {
        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _writeCts, newCts);
        oldCts?.Cancel();
        oldCts?.Dispose();

        var token = newCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token).ConfigureAwait(false);

                await WriteLock.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    // Snapshot to a sorted list without materializing an intermediate dictionary.
                    var snapshot = OffsetCache.ToArray();
                    Array.Sort(snapshot, static (a, b) => string.CompareOrdinal(a.Key, b.Key));

                    var sorted = new Dictionary<string, long>(snapshot.Length, StringComparer.Ordinal);
                    foreach (var kv in snapshot) sorted[kv.Key] = kv.Value;

                    var json = JsonConvert.SerializeObject(sorted);
                    await File.WriteAllTextAsync(OffsetFile, json, token).ConfigureAwait(false);
                    Logger.Debug("OffsetCache written to disk");
                }
                finally
                {
                    WriteLock.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // A newer write was scheduled.
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to write offset cache: {e.Message}");
            }
        }, CancellationToken.None);
    }
}