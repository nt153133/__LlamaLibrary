/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.CodeDom;
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
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using GreyMagic;
using LlamaLibrary.Helpers;
using LlamaLibrary.Hooks;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteAgents;
using Newtonsoft.Json;
using LogLevel = LlamaLibrary.Logging.LogLevel;
using PatchManager = LlamaLibrary.Hooks.PatchManager;

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable

namespace LlamaLibrary.Memory
{
    public static class OffsetManager
    {
        private static readonly StringBuilder Sb = new();

        private static readonly object InitLock = new();
        private static readonly object InitLock1 = new();
        private static bool initStarted;
        private static bool initDone;

        public static Dictionary<string, string> patterns = new();
        public static Dictionary<string, string> constants = new();

        public static ConcurrentDictionary<string, long> OffsetCache = new ConcurrentDictionary<string, long>();

        private static readonly bool _debug = false;

        private static string OffsetFile => Path.Combine(JsonSettings.SettingsPath, $"LL_Offsets_{Core.CurrentGameVer}.json");

        public static LLogger Logger { get; } = new LLogger("LLOffsetManager", Colors.RosyBrown, LogLevel.Information);

        public static void Init()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                lock (InitLock)
                {
                    stopwatch.Restart();
                    if (initDone)
                    {
                        Logger.Debug($"OffsetManager done {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                        return;
                    }

                    if (initStarted)
                    {
                        Logger.Information($"OffsetManager Init started but waiting {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                        while (!initDone)
                        {
                            Thread.Sleep(100);
                        }

                        return;
                    }

                    initStarted = true;
                    lock (InitLock1)
                    {
                        stopwatch = Stopwatch.StartNew();
                        if (initDone)
                        {
                            Logger.Information($"OffsetManager Init started but done now {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                            return;
                        }

                        Logger.Information($"OffsetManager Init started {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");

                        var scriptThread = new Thread(SetScriptsThread);
                        scriptThread.Start();
                        //SetScriptsThread();
                        //newStopwatch.Stop();
                        //Logger.Information($"OffsetManager SetScriptsManager took{newStopwatch.ElapsedMilliseconds}ms");

                        var newStopwatch = Stopwatch.StartNew();

                        var q1 = (from t in Assembly.GetExecutingAssembly().GetTypes()
                                  where t.Namespace != null && t.IsClass && t.Namespace.Contains("LlamaLibrary") && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                                  select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager Init GetTypes took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
                        if (!q1.Contains(typeof(Offsets)))
                        {
                            q1.Add(typeof(Offsets));
                        }

                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager Init Add took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
                        if (File.Exists(OffsetFile))
                        {
                            OffsetCache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, long>>(File.ReadAllText(OffsetFile));
                            if (OffsetCache == null)
                            {
                                OffsetCache = new ConcurrentDictionary<string, long>();
                            }

                            /*else
                            {
                                Logger.Information("Game version: " + Core.CurrentGameVer);
                                Logger.Information("Has key: " + OffsetCache.ContainsKey("LlamaLibrary.RemoteAgents.AgentBagSlot+Offsets.VTable"));
                                if (OffsetCache.ContainsKey("LlamaLibrary.RemoteAgents.AgentBagSlot+Offsets.VTable"))
                                    Logger.Information("Offset: " + OffsetCache["LlamaLibrary.RemoteAgents.AgentBagSlot+Offsets.VTable"]);
                            }*/
                        }

                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager Init Deserialize took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
                        SetOffsetObjects(q1);
                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager SetOffsetObjects took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
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
                        var q = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace != null && t.IsClass && t.Namespace == "LlamaLibrary.RemoteAgents" && typeof(IAgent).IsAssignableFrom(t)).ToList();
                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager GetTypesAgents took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
                        var names = new List<string>();
                        foreach (var MyType in q)
                        {
                            var test = ((IAgent)Activator.CreateInstance(MyType,
                                                                         BindingFlags.Instance | BindingFlags.NonPublic,
                                                                         null,
                                                                         new object[]
                                                                         {
                                                                             IntPtr.Zero
                                                                         },
                                                                         null)
                                ).RegisteredVtable;

                            if (vtables.ContainsKey(test))
                            {
                                names.Add(MyType.Name);
                                Logger.Debug($"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(vtables[test], MyType)}");
                            }
                            else
                            {
                                Logger.Error($"\tFound one {MyType.Name} {test.ToString("X")} but no agent");
                            }
                        }

                        Logger.Information("Added agents: " + string.Join(", ", names));
                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager AgentModule.TryAddAgent took {newStopwatch.ElapsedMilliseconds}ms");

                        newStopwatch.Restart();
                        File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));
                        newStopwatch.Stop();
                        Logger.Debug($"OffsetManager File.WriteAllText took {newStopwatch.ElapsedMilliseconds}ms");

                        if (InventoryUpdatePatch.Offsets.OrginalCall != InventoryUpdatePatch.Offsets.OriginalJump)
                        {
                            Logger.Information("Last patch not cleaned up, cleaning up now");
                            var asm = Core.Memory.Asm;
                            asm.Clear();
                            asm.AddLine("[org 0x{0:X16}]", (ulong)InventoryUpdatePatch.Offsets.PatchLocation);
                            asm.AddLine("JMP {0}", InventoryUpdatePatch.Offsets.OrginalCall);
                            var jzPatch = asm.Assemble();
                            Core.Memory.WriteBytes(InventoryUpdatePatch.Offsets.PatchLocation, jzPatch);
                        }

                        PatchManager.Initialize();
                        //PatchManager.Enable<InventoryUpdatePatch>();
                        Logger.Information($"OffsetManager Init took {stopwatch.ElapsedMilliseconds}ms {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
                        //initStarted = true;
                        if (_debug)
                        {
                            foreach (var pair in OffsetCache)
                            {
                                Logger.Information($"{pair.Key} - {pair.Value:X}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                initDone = true;
                Logger.Debug($"OffsetManager Init took {stopwatch.ElapsedMilliseconds}ms {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}");
            }

            /*
            if (scriptThread != null)
            {
                var newStopwatch1 = Stopwatch.StartNew();
                Logger.Information("Waiting on thread");
                scriptThread.Join();
                newStopwatch1.Stop();
                Logger.Information($"OffsetManager scriptThread.Join took {newStopwatch1.ElapsedMilliseconds}ms");
            }
            */

            stopwatch.Stop();
            Logger.Information($"Total {stopwatch.ElapsedMilliseconds}ms");
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

            if (vtables.ContainsKey(iagent.RegisteredVtable))
            {
                Logger.Information($"\tTrying to add {iagent.GetType()} {AgentModule.TryAddAgent(vtables[iagent.RegisteredVtable], iagent.GetType())}");
            }
            else
            {
                Logger.Error($"\tFound one {iagent.RegisteredVtable.ToString("X")} but no agent");
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

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public static void SetOffsetObjects(IEnumerable<Type> q1)
        {
            var types = q1.SelectMany(j => j.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public));

            using var pf = new PatternFinder(Core.Memory);
            Parallel.ForEach(types,
                             type =>
                             {
                                 if (type.FieldType.IsClass)
                                 {
                                     var instance = Activator.CreateInstance(type.FieldType);

                                     foreach (var field in type.FieldType.GetFields(BindingFlags.Instance))
                                     {
                                         //Logger.Information("Trying to set " + field.Name);
                                         var res = ParseField(field, pf);
                                         if (field.FieldType == typeof(IntPtr))
                                         {
                                             field.SetValue(instance, res);
                                         }
                                         else
                                         {
                                             field.SetValue(instance, (int)res);
                                         }
                                     }

                                     //set the value
                                     type.SetValue(null, instance);
                                 }
                                 else
                                 {
                                     //Logger.Information("Trying to set " + type.Name);
                                     var res = ParseField(type, pf);
                                     if (type.FieldType == typeof(IntPtr))
                                     {
                                         type.SetValue(null, res);
                                     }
                                     else
                                     {
                                         try
                                         {
                                             type.SetValue(null, res.ToInt32());
                                         }
                                         catch (Exception)
                                         {
                                             Logger.Error($"Error on {type.Name}");
                                         }
                                     }
                                 }
                             });
        }

        private static IntPtr ParseField(FieldInfo field, PatternFinder pf)
        {
            var offset = (OffsetAttribute?)Attribute.GetCustomAttributes(field, typeof(OffsetAttribute)).FirstOrDefault();

            var valna = (OffsetValueNA?)Attribute.GetCustomAttributes(field, typeof(OffsetValueNA)).FirstOrDefault();

            var result = IntPtr.Zero;
            var name = $"{field.DeclaringType?.FullName}.{field.Name}";
            //var lang = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic).First(i => i.FieldType == typeof(Language)).GetValue(null);

            if (Translator.Language == Language.Chn) //Translator.Language
            {
                var offsetCN = (OffsetCNAttribute?)Attribute.GetCustomAttributes(field, typeof(OffsetCNAttribute)).FirstOrDefault();
                var valcn = (OffsetValueCN?)Attribute.GetCustomAttributes(field, typeof(OffsetValueCN)).FirstOrDefault();

                if (valcn != null)
                {
                    return (IntPtr)valcn.Value;
                }

                if (offset == null)
                {
                    return IntPtr.Zero;
                }

                //var b1 = true;
                try
                {
                    if (OffsetCache.ContainsKey(name) && !offset.IgnoreCache)
                    {
                        //Logger.Information("Found in cache");
                        var offsetVal = OffsetCache[name];
                        if (field.FieldType != typeof(int))
                        {
                            result = Core.Memory.GetAbsolute(new IntPtr(offsetVal));
                        }
                        else
                        {
                            result = new IntPtr(offsetVal);
                        }
                    }
                    else
                    {
                        //Logger.Information($"Not found in cache : {field.DeclaringType.FullName}.{field.Name}");
                        result = pf.FindSingle(offsetCN != null ? offsetCN.PatternCN : offset.Pattern, true);
                        //result = pf.Find(offset.Pattern);
                        if (result != IntPtr.Zero)
                        {
                            if (field.FieldType != typeof(int))
                            {
                                OffsetCache.TryAdd($"{field.DeclaringType?.FullName}.{field.Name}", Core.Memory.GetRelative(result).ToInt64());
                            }
                            else
                            {
                                OffsetCache.TryAdd($"{field.DeclaringType?.FullName}.{field.Name}", result.ToInt64());
                            }
                        }
                    }
                    //result = pf.Find(offsetCN != null ? offsetCN.PatternCN : offset.Pattern);
                }
                catch (Exception e)
                {
                    if (field.DeclaringType != null && field.DeclaringType.IsNested)
                    {
                        Logger.Error($"[{field.DeclaringType?.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                        Logger.Exception(e);
                    }
                    else
                    {
                        Logger.Error($"[{field.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                        Logger.Exception(e);
                    }
                }
            }
            else
            {
                if (valna != null)
                {
                    return (IntPtr)valna.Value;
                }

                if (offset == null)
                {
                    return IntPtr.Zero;
                }

                try
                {
                    //Logger.Information("Offsetcache Count: " + OffsetCache.Count);
                    if (OffsetCache.ContainsKey(name) && !offset.IgnoreCache)
                    {
                        //Logger.Information("Found in cache");
                        var offsetVal = OffsetCache[name];
                        if (field.FieldType != typeof(int))
                        {
                            result = Core.Memory.GetAbsolute(new IntPtr(offsetVal));
                        }
                        else
                        {
                            result = new IntPtr(offsetVal);
                        }
                    }
                    else
                    {
                        //Logger.Information($"Not found in cache : {field.DeclaringType.FullName}.{field.Name}");
                        result = pf.FindSingle(offset.Pattern, true);
                        //result = pf.Find(offset.Pattern);
                        if (result != IntPtr.Zero)
                        {
                            if (field.FieldType != typeof(int))
                            {
                                OffsetCache.TryAdd($"{field.DeclaringType?.FullName}.{field.Name}", Core.Memory.GetRelative(result).ToInt64());
                            }
                            else
                            {
                                OffsetCache.TryAdd($"{field.DeclaringType?.FullName}.{field.Name}", result.ToInt64());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (field.DeclaringType != null && field.DeclaringType.IsNested)
                    {
                        Logger.Error($"[{field.DeclaringType.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                        Logger.Exception(e);
                    }
                    else
                    {
                        Logger.Error($"[{field.DeclaringType?.Name}:{field.Name:,27}] Not Found");
                        Logger.Exception(e);
                    }
                }
            }

            /*
            if (result == IntPtr.Zero)
            {
                if(field.DeclaringType != null && field.DeclaringType.IsNested)
                    Log.Error($"[{field.DeclaringType.DeclaringType.Name}:{field.Name:,27}] Not Found");
                else
                {
                    Log.Error($"[{field.DeclaringType.Name}:{field.Name:,27}] Not Found");
                }
            }
            */

            if (!_debug)
            {
                return result;
            }

            switch (field.DeclaringType)
            {
                //Sb.AppendLine($"{field.DeclaringType.FullName}.{field.Name}");
                case { IsNested: true } when field.FieldType != typeof(int):
                    Sb.AppendLine($"{field.DeclaringType?.DeclaringType?.Name}_{field.Name}, {offset.Pattern} - {offset.PatternCN}");
                    patterns.Add($"{field.DeclaringType?.DeclaringType?.Name}_{field.Name}", offset.Pattern);
                    break;
                case { IsNested: true } when field.FieldType == typeof(int):
                    //sb.AppendLine($"{field.DeclaringType.DeclaringType.Name}_{field.Name}, {offset.Pattern} - {offset.PatternCN}");
                    constants.Add($"{field.DeclaringType?.DeclaringType?.Name}_{field.Name}", offset.Pattern);
                    break;
                default:
                {
                    if (field.FieldType != typeof(int))
                    {
                        Sb.AppendLine($"{field.Name}, {offset.Pattern}"); // - {offset.PatternCN}
                        patterns.Add($"{field.Name}", offset.Pattern);
                    }
                    else
                    {
                        Sb.AppendLine($"{field.Name}, {offset.Pattern} "); //- {offsetCN?.PatternCN}
                        constants.Add($"{field.Name}", offset.Pattern);
                    }

                    break;
                }
            }

            if (valna != null)
            {
                Sb.AppendLine($"{field.DeclaringType?.Name},{field.Name},{valna}");
            }

            if (field.DeclaringType is { IsNested: true })
            {
                Logger.Information($"[{field.DeclaringType?.DeclaringType?.Name}:{field.Name:,27}] {result.ToInt64():X}");
            }
            else
            {
                Logger.Information($"[{field.DeclaringType?.Name}:{field.Name:,27}] {result.ToInt64():X}");
            }

            return result;
        }

        private static void SetScriptsThread()
        {
            // AddNamespacesToScriptManager(new[] { "LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras" }); //
            ScriptManager.AddNamespaces("LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras");
            ScriptManager.Init(typeof(ScriptConditions.Helpers));
            Logger.Information("ScriptManager Set");
        }

        public static string? GetRootNamespace(string? nameSpace)
        {
            return nameSpace != null && nameSpace.IndexOf('.') > 0 ? nameSpace.Substring(0, nameSpace.IndexOf('.')) : nameSpace;
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static string? GetCurrentNamespace()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            return GetRootNamespace(type?.Namespace);
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static List<Type> GetOffsetClasses()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType?.Assembly.GetTypes()
                      where t.Namespace != null && t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            return q1;
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetOffsetClasses()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType?.Assembly.GetTypes()
                      where t.Namespace != null && t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            SetOffsetObjects(q1);

            while (!initDone)
            {
                Thread.Sleep(100);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetOffsetClassesAndAgents()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType?.Assembly.GetTypes()
                      where t.Namespace != null && t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets")
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            SetOffsetObjects(q1);

            File.WriteAllText(OffsetFile, JsonConvert.SerializeObject(OffsetCache));

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

            var q = from t in method.DeclaringType?.Assembly.GetTypes()
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
                                                             null)
                    ).RegisteredVtable;

                if (vtables.ContainsKey(test))
                {
                    Logger.WriteLog(Colors.BlueViolet, $"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(vtables[test], MyType)}");
                }
                else
                {
                    Logger.WriteLog(Colors.BlueViolet, $"\tFound one {MyType.Name} {test.ToString("X")} but no agent");
                }
            }

            while (!initDone)
            {
                Thread.Sleep(100);
            }
        }
    }
}