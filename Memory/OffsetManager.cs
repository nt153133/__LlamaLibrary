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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot;
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
using PatchManager = LlamaLibrary.Hooks.PatchManager;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AccessToDisposedClosure

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable

namespace LlamaLibrary.Memory;

public static class OffsetManager
{
    private static readonly StringBuilder Sb = new();

    //private static readonly SemaphoreSlim InitLock = new SemaphoreSlim(1, 1);
    //private static readonly SemaphoreSlim InitLock1 = new(1, 1);
    //private static bool initStarted;
    private static bool initDone;

    public static readonly Dictionary<string, string> patterns = new();
    public static readonly Dictionary<string, string> constants = new();

    public static ConcurrentDictionary<string, long> OffsetCache = new();

    private const long _version = 34;

    private const bool _debug = false;

    private static int GameVersion
    {
        get
        {
            if (GameVersion1 != 0)
            {
                return GameVersion1;
            }

            try
            {
                GameVersion1 = Core.CurrentGameVer;
            }
            catch
            {
                GameVersion1 = 0;
            }

            return GameVersion1;
        }
    }

    private static string OffsetFile { get; } = Path.Combine(JsonSettings.SettingsPath, $"LL_Offsets_{GameVersion}.json");

    public static LLogger Logger { get; } = new("LLOffsetManager", Colors.RosyBrown);

#if RB_CN
    public static float CurrentGameVersion = 7.1f;
    public static bool IsChinese = true;
    public static bool Is7_1 = true;
    public static bool Is7_15 = false;
    public static bool Is7_16 = false;
#else
    public static float CurrentGameVersion = 7.2f;
    public static bool IsChinese = false;
    public static bool Is7_1 = true;
    public static bool Is7_15 = true;
    public static bool Is7_16 = true;
#endif

#if RB_DT
        public static bool IsDawntrail = true;
#else
    public static bool IsDawntrail = false;
#endif

    private static bool _isNewGameBuild;
    private static int GameVersion1;

    [Obsolete]
    public static void Init()
    {
        //Logger.Information($"Is init done {initDone}");
    }

    public static async Task<bool> InitLib()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            try
            {
                stopwatch.Restart();
                /*if (initDone)
                {
                    Logger.Debug($"OffsetManager done {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                    return true;
                }

                if (initStarted)
                {
                    Logger.Information($"OffsetManager Init started but waiting {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                    while (!initDone)
                    {
                        await Task.Delay(100);
                    }

                    return true;
                }*/

                //initStarted = true;

                stopwatch = Stopwatch.StartNew();
                /*
                if (initDone)
                {
                    Logger.Information($"OffsetManager Init started but done now {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                    return true;
                }

                Logger.Information($"OffsetManager Init started {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                */

                var newStopwatch = Stopwatch.StartNew();

                await SearchAndSetLL();
                newStopwatch.Stop();

                Logger.Debug($"OffsetManager SearchAndSetLL took {newStopwatch.ElapsedMilliseconds}ms");

                newStopwatch.Restart();

                Logger.Information($"OffsetManager Init took {stopwatch.ElapsedMilliseconds}ms {new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.Name}");

                PrintLastCommit();

                Logger.Information($"Dalamud Dectected: {GeneralFunctions.DalamudDetected()}");
            }
            finally
            {
                initDone = true;
                //initStarted = false;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
        finally
        {
            initDone = true;
            stopwatch.Stop();
            Logger.Debug($"OffsetManager Init took {stopwatch.ElapsedMilliseconds}ms");
        }

        return true;
    }

    private static void PrintLastCommit()
    {
        var lastCommitfile = Path.Combine(GeneralFunctions.SourceDirectory()?.Parent?.FullName ?? string.Empty, "LastCommit.txt");
        if (!File.Exists(lastCommitfile))
        {
            return;
        }

        var lastCommit = File.ReadAllText(lastCommitfile).Trim();
        if (DateTime.TryParse(lastCommit, out var result))
        {
            Logger.Information($"Last Commit: {result.ToUniversalTime():ddd, dd MMM yyy HH:mm:ss ‘UTC’}");
            Logger.Information($"Raw Last Commit: {lastCommit}");
        }
        else
        {
            Logger.Information($"Last Commit: '{lastCommit}'");
        }
    }

    private static async Task SearchAndSetLL()
    {
        var llTypes = GetTypes();

        if (File.Exists(OffsetFile) && GameVersion != 0)
        {
            OffsetCache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, long>>(await File.ReadAllTextAsync(OffsetFile)) ?? new ConcurrentDictionary<string, long>();
            if (!OffsetCache.TryGetValue("Version", out var value) || value != _version)
            {
                OffsetCache.Clear();
                value = _version;
                OffsetCache["Version"] = value;
                _isNewGameBuild = true;
            }
        }

        await SetOffsetObjectsAsync(llTypes);
    }

    internal static void ClearOffsetFromCache(MemberInfo? info)
    {
        if (info == null)
        {
            Logger.Error("MemberInfo is null");
            return;
        }

        OffsetCache.TryRemove(info.MemberName(), out _);
        if (GameVersion != 0)
        {
            File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));
        }
    }

    internal static void ClearOffsetFromCache(string name)
    {
        OffsetCache.TryRemove(name, out _);
        File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));
    }

    private static List<Type> GetTypes()
    {
        var q1 = (from t in Assembly.GetExecutingAssembly().GetTypes()
                  where t.Namespace != null && t.IsClass && t.Namespace.Contains("LlamaLibrary") && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

        if (!q1.Contains(typeof(Offsets)))
        {
            q1.Add(typeof(Offsets));
        }

        return q1;
    }

    internal static void SetPostOffsets()
    {
        var newStopwatch = Stopwatch.StartNew();
        var vtables = new Dictionary<IntPtr, int>();
        var pointers = AgentModule.AgentVtables;
        for (var index = 0; index < pointers.Count; index++)
        {
            if (vtables.ContainsKey(pointers[index]))
            {
                continue;
            }

            vtables.Add(pointers[index], index);
        }

        Logger.Debug($"OffsetManager AgentModule.AgentVtables took {newStopwatch.ElapsedMilliseconds}ms");
        var q = Assembly.GetExecutingAssembly().GetTypes().Where(t => t is { Namespace: "LlamaLibrary.RemoteAgents", IsClass: true } && typeof(IAgent).IsAssignableFrom(t)).ToList();
        newStopwatch.Stop();
        Logger.Debug($"OffsetManager GetTypesAgents took {newStopwatch.ElapsedMilliseconds}ms");

        newStopwatch.Restart();
        var names = new List<string>();
        foreach (var myType in q)
        {
            var test = ((IAgent)Activator.CreateInstance(myType,
                                                         BindingFlags.Instance | BindingFlags.NonPublic,
                                                         null,
                                                         new object[]
                                                         {
                                                             IntPtr.Zero
                                                         },
                                                         null)!).RegisteredVtable;

            if (vtables.TryGetValue(test, out var vtable))
            {
                names.Add(myType.Name);
                Logger.Debug($"\tTrying to add {myType.Name} {AgentModule.TryAddAgent(vtable, myType)}");
            }
            else
            {
                Logger.Error($"\tFound one {myType.Name} {test:X} but no agent");
            }
        }

        Logger.Information($"Added {names.Count} agents");
        newStopwatch.Stop();
        Logger.Debug($"OffsetManager AgentModule.TryAddAgent took {newStopwatch.ElapsedMilliseconds}ms");

        if (LlamaLibrarySettings.Instance.TempDisableInventoryHook && InventoryUpdatePatch.Offsets.OrginalCall == InventoryUpdatePatch.Offsets.OriginalJump)
        {
            LlamaLibrarySettings.Instance.TempDisableInventoryHook = false;
        }

        if (GameVersion != LlamaLibrarySettings.Instance.LastRevision && InventoryUpdatePatch.Offsets.OrginalCall == InventoryUpdatePatch.Offsets.OriginalJump)
        {
            LlamaLibrarySettings.Instance.LastRevision = GameVersion;
            Logger.Information($"Setting revision to {GameVersion} in {LlamaLibrarySettings.Instance.FilePath}");
        }

        var skipInventoryPatch = LlamaLibrarySettings.Instance.TempDisableInventoryHook || LlamaLibrarySettings.Instance.DisableInventoryHook || LibraryClass.SafeMode;

        Logger.Information($"TempDisableInventoryHook: {LlamaLibrarySettings.Instance.TempDisableInventoryHook} DisableInventoryHook: {LlamaLibrarySettings.Instance.DisableInventoryHook}");

        if (!skipInventoryPatch)
        {
            if (InventoryUpdatePatch.Offsets.OrginalCall != InventoryUpdatePatch.Offsets.OriginalJump || InventoryUpdatePatch.Offsets.OrginalCall == IntPtr.Zero || InventoryUpdatePatch.Offsets.OriginalJump == IntPtr.Zero)
            {
                if (!_isNewGameBuild && InventoryUpdatePatch.Offsets.OrginalCall != IntPtr.Zero && InventoryUpdatePatch.Offsets.OriginalJump != IntPtr.Zero)
                {
                    Logger.Information("Last patch not cleaned up, cleaning up now");
                    var asm = Core.Memory.Asm;
                    asm.Clear();
                    asm.AddLine("[org 0x{0:X16}]", (ulong)InventoryUpdatePatch.Offsets.PatchLocation);
                    asm.AddLine("JMP {0}", InventoryUpdatePatch.Offsets.OrginalCall);
                    var jzPatch = asm.Assemble();
                    Core.Memory.WriteBytes(InventoryUpdatePatch.Offsets.PatchLocation, jzPatch);
                    InventoryUpdatePatch.Offsets.OriginalJump = InventoryUpdatePatch.Offsets.OrginalCall;
                }
                else
                {
                    Logger.Error("New game build and inventory patch offsets don't match");
                    var memberInfo = typeof(InventoryUpdatePatch.Offsets).GetMember("OrginalCall", BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault();
                    ClearOffsetFromCache(memberInfo);
                    LlamaLibrarySettings.Instance.TempDisableInventoryHook = true;
                    skipInventoryPatch = true;
                }
            }
        }

        newStopwatch.Restart();
        if (GameVersion != 0)
        {
            File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));
        }

        newStopwatch.Stop();
        Logger.Debug($"OffsetManager File.WriteAllText took {newStopwatch.ElapsedMilliseconds}ms");

        PatchManager.Initialize(skipInventoryPatch);
    }

    public static void RegisterAgent(IAgent iagent)
    {
        var vtables = new Dictionary<IntPtr, int>();
        var pointers = AgentModule.AgentVtables;
        for (var index = 0; index < pointers.Count; index++)
        {
            if (vtables.ContainsKey(pointers[index]))
            {
                continue;
            }

            vtables.Add(pointers[index], index);
        }

        if (vtables.TryGetValue(iagent.RegisteredVtable, out var vtable))
        {
            Logger.Information($"\tTrying to add {iagent.GetType()} {AgentModule.TryAddAgent(vtable, iagent.GetType())}");
        }
        else
        {
            Logger.Error($"\tFound one {iagent.RegisteredVtable:X} but no agent");
        }
    }

    internal static void AddNamespacesToScriptManager(params string[] param)
    {
        var field =
            typeof(ScriptManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType == typeof(List<string>));

        if (field == null)
        {
            return;
        }

        try
        {
            if (field.GetValue(null) is not List<string> list)
            {
                return;
            }

            foreach (var ns in param)
            {
                if (!list.Contains(ns))
                {
                    list.Add(ns);
                    Logger.Information($"Added namespace '{ns}' to ScriptManager");
                }
            }
        }
        catch
        {
            Logger.Error("Failed to add namespaces to ScriptManager, this can cause issues with some profiles.");
        }
    }

    public static IEnumerable<MemberInfo> MemberInfos(Type j)
    {
        IEnumerable<MemberInfo> infos = j.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).Where(i => !i.IsInitOnly && !i.IsPrivate);
        return infos.Concat(j.GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).Where(i => i.CanWrite));
    }

    public static IEnumerable<MemberInfo> MemberInfos(IEnumerable<Type> j)
    {
        return j.SelectMany(MemberInfos);
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static async Task SetOffsetObjectsAsync(IEnumerable<Type> q1)
    {
        var types = MemberInfos(q1).ToList();

        List<MemberInfo> fields = new();
        var foundAll = true;
        foreach (var memberInfo in types)
        {
            var type = memberInfo.GetMemberType();
            if (type == null)
            {
                Logger.Error($"Parsing class {memberInfo.Name}, this shouldn't happen");
            }
            else
            {
                //Logger.Information($"Parsing field {type.Name}");

                if (!memberInfo.IgnoreCache() && OffsetCache.TryGetValue(memberInfo.MemberName(), out var offsetVal))
                {
                    memberInfo.SetValue(offsetVal);
                    continue;
                }

                //Logger.Information($"{(offset.IgnoreCache ? "Skipping cache" : "Not found in cache" )} : {type.DeclaringType.FullName}.{type.Name}");
                fields.Add(memberInfo);

                foundAll = false;
            }
        }

        if (foundAll)
        {
            Logger.Information("All offsets found in cache");
            return;
        }

        Logger.Information($"Not all ({fields.Count}) offsets found in cache");

        var pf = PatternFinderProxy.PatternFinder;

        //Take the list of types and search for the offsets using multiple tasks
        var tasks = fields.Select(type => Task.Run(() => SearchOffset(type, pf))).ToList();

        //Wait for all tasks to complete
        await Task.WhenAll(tasks);
    }

    public static void SetOffsetObjects(IEnumerable<Type> q1)
    {
        var types = MemberInfos(q1);

        var pf = PatternFinderProxy.PatternFinder;
        Parallel.ForEach(types, type => { SearchOffset(type, pf); });
    }

    private static void SearchOffset(MemberInfo info, ISearcher pf)
    {
        var type = info.GetMemberType();

        //Logger.Information($"{info.Name} - {info.DeclaringType} - {info.ReflectedType}");

        if (type is { IsClass: true, IsAbstract: false })
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
                    if (field.DeclaringType != null && field.DeclaringType.IsNested)
                    {
                        Logger.Error($"[{field.DeclaringType?.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                    }
                    else
                    {
                        Logger.Error($"[{field.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                    }

                    Logger.Error($"{field.GetPattern()}");

                    continue;
                }

                field.SetValue(instance, result);
                Logger.Information($"Setting {result.ToString("X")} to {instance.DynamicString()}");
            }

            //set the value
            info.SetValue(null, instance);
        }
        else
        {
            //Logger.Information("Trying to set " + type.Name);
            var result = info.GetOffset(pf);
            if (result == IntPtr.Zero)
            {
                if (info.DeclaringType != null && info.DeclaringType.IsNested)
                {
                    Logger.Error($"[{info.DeclaringType?.DeclaringType?.Name}:{info.Name:,27}] Not Found");
                }
                else
                {
                    Logger.Error($"[{info.DeclaringType?.Name}:{info.Name:,27}] Not Found");
                }

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
    }

    internal static void SetScriptsThread()
    {
        // AddNamespacesToScriptManager(new[] { "LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras" }); //
        Logger.Information("Setting ScriptManager");
        Task.Run(() =>
        {
            ScriptManager.AddNamespaces("LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras");
            ScriptManager.Init(typeof(ScriptConditions.Helpers));
            Logger.Information("ScriptManager Set");
        });
    }

    public static string? GetRootNamespace(string? nameSpace)
    {
        return nameSpace != null && nameSpace.IndexOf('.') > 0 ? nameSpace.Substring(0, nameSpace.IndexOf('.')) : nameSpace;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string? GetCurrentNamespace()
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var type = method?.DeclaringType;
        return GetRootNamespace(type?.Namespace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static List<Type> GetOffsetClasses()
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var type = method?.DeclaringType;

        var q1 = (from t in method?.DeclaringType?.Assembly.GetTypes()
                  where t.Namespace != null && t.IsClass && t.Namespace!.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

        return q1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetOffsetClasses()
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var type = method?.DeclaringType;

        var q1 = (from t in method?.DeclaringType?.Assembly.GetTypes()
                  where t.Namespace != null && t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

        SetOffsetObjects(q1);

        while (!initDone)
        {
            Thread.Sleep(100);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetOffsetClassesAndAgents()
    {
        var frame = new StackFrame(1);
        var method = frame.GetMethod();
        var type = method?.DeclaringType;

        var q1 = (from t in method?.DeclaringType?.Assembly.GetTypes()
                  where t.Namespace != null && t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

        SetOffsetObjects(q1);

        if (GameVersion != 0)
        {
            File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));
        }

        var vtables = new Dictionary<IntPtr, int>();
        var pointers = AgentModule.AgentVtables;

        for (var index = 0; index < pointers.Count; index++)
        {
            if (vtables.ContainsKey(pointers[index]))
            {
                continue;
            }

            vtables.Add(pointers[index], index);
        }

        var q = from t in method?.DeclaringType?.Assembly.GetTypes()
                where t.IsClass && typeof(IAgent).IsAssignableFrom(t)
                select t;

        foreach (var MyType in q.Where(i => typeof(IAgent).IsAssignableFrom(i)))
        {
            var test = ((IAgent)Activator.CreateInstance(MyType,
                                                         BindingFlags.Instance | BindingFlags.NonPublic,
                                                         null,
                                                         new object[]
                                                         {
                                                             IntPtr.Zero
                                                         },
                                                         null)!).RegisteredVtable;

            if (vtables.TryGetValue(test, out var value))
            {
                Logger.WriteLog(Colors.BlueViolet, $"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(value, MyType)}");
            }
            else
            {
                Logger.WriteLog(Colors.BlueViolet, $"\tFound one {MyType.Name} {test:X} but no agent");
            }
        }

        while (!initDone)
        {
            Thread.Sleep(100);
        }
    }

    public static Dictionary<string, string> LLDict()
    {
        var results = new Dictionary<string, string>();

        // var asm = Assembly.Load("LlamaLibrary.dll");
        var q1 = (from t in typeof(OffsetAttribute).Assembly.GetTypes()
                  where t.Namespace != null && (t.IsClass && t.Namespace.Contains("LlamaLibrary") && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets"))
                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

        if (!q1.Contains(typeof(Offsets)))
        {
            //q1.Add(typeof(Offsets));
        }

        var types = MemberInfos(q1).ToList();

        Logger.Information($"{types.Count}");

        foreach (var field in types)
        {
            if (field.DeclaringType != null && field.DeclaringType.IsNested)
            {
                try
                {
                    if (field.DeclaringType.DeclaringType != null)
                    {
                        Logger.Information($"{field.DeclaringType.DeclaringType.Name}_{field.Name:,27},{field.GetPattern(ForceClientMode.Global)}");
                        results.Add($"{field.DeclaringType.DeclaringType.Name}_{field.Name}", field.GetPattern(ForceClientMode.Global));
                    }
                }
                catch (Exception e)
                {
                    Logger.Information($"\t{field.DeclaringType.Name}_{field.Name} Issue");
                    Console.WriteLine(e);
                    //throw;
                }
            }
            else
            {
                Logger.Information($"{field.DeclaringType?.Name}_{field.Name:,27},{field.GetPattern(ForceClientMode.Global)}");
                try
                {
                    results.Add($"{field.DeclaringType?.Name}_{field.Name}", field.GetPattern(ForceClientMode.Global));
                }
                catch (Exception e)
                {
                    Logger.Information($"\t{field.DeclaringType?.Name}_{field.Name} DUPE");
                    Console.WriteLine(e);
                    //throw;
                }
            }
        }

        return results;
    }

    public static Dictionary<string, string> LLDictCN()
    {
        var results = new Dictionary<string, string>();

        // var asm = Assembly.Load("LlamaLibrary.dll");
        var q1 = GetTypes();

        if (!q1.Contains(typeof(Offsets)))
        {
            q1.Add(typeof(Offsets));
        }

        var types = MemberInfos(q1).ToList();

        Logger.Information($"{types.Count}");

        foreach (var field in types)
        {
            if (field.DeclaringType != null && field.DeclaringType.IsNested)
            {
                Logger.Information($"CN{field.DeclaringType.DeclaringType?.Name}_{field.Name:,27},{field.GetPattern(ForceClientMode.CN)}");
                results.Add($"{field.DeclaringType.DeclaringType?.Name}_{field.Name}", field.GetPattern(ForceClientMode.CN));
            }
            else
            {
                Logger.Information($"CN{field.DeclaringType?.Name}_{field.Name:,27},{field.GetPattern(ForceClientMode.CN)}");
                results.Add($"{field.DeclaringType?.Name}_{field.Name}", field.GetPattern(ForceClientMode.CN));
            }
        }

        return results;
    }
}