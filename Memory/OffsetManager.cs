//!CompilerOption:optimize
/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using GreyMagic;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.Memory
{
    public static class OffsetManager
    {
        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly string Name = "LLOffsetManager";
        private static readonly Color LogColor = Colors.RosyBrown;
        private static readonly LLogger Log = new LLogger(Name, LogColor);

        private static bool initDone;
        private static object initLock = new object();
        public static Dictionary<string, string> patterns = new Dictionary<string, string>();
        public static Dictionary<string, string> constants = new Dictionary<string, string>();

        private static readonly bool _debug = false;

        public static LLogger Log1 => Log;

        public static void Init()
        {
            lock (initLock)
            {
                if (initDone)
                {
                    return;
                }

                initDone = true;

                var q1 = (from t in Assembly.GetExecutingAssembly().GetTypes()
                          where t.Namespace != null && (t.IsClass && t.Namespace.Contains("LlamaLibrary") && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets"))
                          select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

                if (!q1.Contains(typeof(Offsets)))
                {
                    q1.Add(typeof(Offsets));
                }

                SetOffsetObjects(q1);

                var vtables = new Dictionary<IntPtr, int>();
                for (var index = 0; index < AgentModule.AgentVtables.Count; index++)
                {
                    vtables.Add(AgentModule.AgentVtables[index], index);
                }

                var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsClass && t.Namespace == "LlamaLibrary.RemoteAgents"
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
                        Log1.Information($"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(vtables[test], MyType)}");
                    }
                    else
                    {
                        Log1.Error($"\tFound one {MyType.Name} {test.ToString("X")} but no agent");
                    }
                }

                AddNamespacesToScriptManager(new[] { "LlamaLibrary", "LlamaLibrary.ScriptConditions", "LlamaLibrary.ScriptConditions.Helpers", "LlamaLibrary.ScriptConditions.Extras" }); //
                ScriptManager.Init(typeof(ScriptConditions.Helpers));
                initDone = true;
                if (_debug)
                {
                    Log1.Information($"\n {Sb}");
                }
            }
        }

        public static void RegisterAgent(IAgent iagent)
        {
            var vtables = new Dictionary<IntPtr, int>();
            for (var index = 0; index < AgentModule.AgentVtables.Count; index++)
            {
                vtables.Add(AgentModule.AgentVtables[index], index);
            }

            if (vtables.ContainsKey(iagent.RegisteredVtable))
            {
                Log1.Information($"\tTrying to add {iagent.GetType()} {AgentModule.TryAddAgent(vtables[iagent.RegisteredVtable], iagent.GetType())}");
            }
            else
            {
                Log1.Error($"\tFound one {iagent.RegisteredVtable.ToString("X")} but no agent");
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
                if (!(field.GetValue(null) is List<string> list))
                {
                    return;
                }

                foreach (var ns in param)
                {
                    if (!list.Contains(ns))
                    {
                        list.Add(ns);
                        Log1.Information($"Added namespace '{ns}' to ScriptManager");
                    }
                }
            }
            catch
            {
                Log1.Error("Failed to add namespaces to ScriptManager, this can cause issues with some profiles.");
            }
        }

        public static void SetOffsetObjects(IEnumerable<Type> q1)
        {
            var types = q1.SelectMany(j => j.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public));

            using (var pf = new PatternFinder(Core.Memory))
            {
                Parallel.ForEach(types, type =>
                                 {
                                     if (type.FieldType.IsClass)
                                     {
                                         var instance = Activator.CreateInstance(type.FieldType);

                                         foreach (var field in type.FieldType.GetFields(BindingFlags.Instance))
                                         {
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
                                                 Log1.Error($"Error on {type.Name}");
                                             }
                                         }
                                     }
                                 }
                                );
            }
        }

        private static IntPtr ParseField(FieldInfo field, PatternFinder pf)
        {
            var offset = (OffsetAttribute)Attribute.GetCustomAttributes(field, typeof(OffsetAttribute))
                .FirstOrDefault();
            var offsetCN = (OffsetCNAttribute)Attribute.GetCustomAttributes(field, typeof(OffsetCNAttribute))
                .FirstOrDefault();
            var valcn = (OffsetValueCN)Attribute.GetCustomAttributes(field, typeof(OffsetValueCN))
                .FirstOrDefault();
            var valna = (OffsetValueNA)Attribute.GetCustomAttributes(field, typeof(OffsetValueNA))
                .FirstOrDefault();

            var result = IntPtr.Zero;
            var lang = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);

            if (lang == Language.Chn)
            {
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
                    result = pf.Find(offsetCN != null ? offsetCN.PatternCN : offset.Pattern);
                }
                catch (Exception)
                {
                    if (field.DeclaringType != null && field.DeclaringType.IsNested)
                    {
                        Log1.Error($"[{field.DeclaringType.DeclaringType.Name}:{field.Name:,27}] Not Found");
                    }
                    else
                    {
                        Log1.Error($"[{field.DeclaringType.Name}:{field.Name:,27}] Not Found");
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
                    result = pf.Find(offset.Pattern);
                }
                catch (Exception)
                {
                    if (field.DeclaringType != null && field.DeclaringType.IsNested)
                    {
                        Log1.Error($"[{field.DeclaringType.DeclaringType.Name}:{field.Name:,27}] Not Found");
                    }
                    else
                    {
                        Log1.Error($"[{field.DeclaringType.Name}:{field.Name:,27}] Not Found");
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

            if (offset != null)
            {
                if (field.DeclaringType != null && field.DeclaringType.IsNested && field.FieldType != typeof(int))
                {
                    Sb.AppendLine($"{field.DeclaringType.DeclaringType.Name}_{field.Name}, {offset.Pattern} - {offset.PatternCN}");
                    patterns.Add($"{field.DeclaringType.DeclaringType.Name}_{field.Name}", offset.Pattern);
                }
                else if (field.DeclaringType != null && field.DeclaringType.IsNested && field.FieldType == typeof(int))
                {
                    //sb.AppendLine($"{field.DeclaringType.DeclaringType.Name}_{field.Name}, {offset.Pattern} - {offset.PatternCN}");
                    constants.Add($"{field.DeclaringType.DeclaringType.Name}_{field.Name}", offset.Pattern);
                }
                else if (field.FieldType != typeof(int))
                {
                    Sb.AppendLine($"{field.Name}, {offset.Pattern} - {offset.PatternCN}");
                    patterns.Add($"{field.Name}", offset.Pattern);
                }
                else
                {
                    Sb.AppendLine($"{field.Name}, {offset.Pattern} - {offsetCN?.PatternCN}");
                    constants.Add($"{field.Name}", offset.Pattern);
                }
            }

            if (valna != null)
            {
                Sb.AppendLine($"{field.DeclaringType.Name},{field.Name},{valna}");
            }

            if (field.DeclaringType != null && field.DeclaringType.IsNested)
            {
                Log1.Information($"[{field.DeclaringType.DeclaringType.Name}:{field.Name:,27}] {result.ToInt64():X}");
            }
            else
            {
                Log1.Information($"[{field.DeclaringType.Name}:{field.Name:,27}] {result.ToInt64():X}");
            }

            return result;
        }

        public static string GetRootNamespace(string nameSpace)
        {
            return nameSpace.IndexOf('.') > 0 ? nameSpace.Substring(0, nameSpace.IndexOf('.')) : nameSpace;
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentNamespace()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            return GetRootNamespace(type.Namespace);
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static List<Type> GetOffsetClasses()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType.Assembly.GetTypes()
                      where t.Namespace != null && (t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets"))
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            return q1;
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetOffsetClasses()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType.Assembly.GetTypes()
                      where t.Namespace != null && (t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets"))
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            SetOffsetObjects(q1);
        }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetOffsetClassesAndAgents()
        {
            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;

            var q1 = (from t in method.DeclaringType.Assembly.GetTypes()
                      where t.Namespace != null && (t.IsClass && t.Namespace.Contains(GetRootNamespace(type.Namespace)) && t.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).Any(i => i.Name == "Offsets"))
                      select t.GetNestedType("Offsets", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public)).ToList();

            SetOffsetObjects(q1);

            var vtables = new Dictionary<IntPtr, int>();
            for (var index = 0; index < AgentModule.AgentVtables.Count; index++)
            {
                vtables.Add(AgentModule.AgentVtables[index], index);
            }

            var q = from t in method.DeclaringType.Assembly.GetTypes()
                    where t.IsClass && typeof(IAgent).IsAssignableFrom(t)
                    select t;

            foreach (var MyType in q.Where(i => typeof(IAgent).IsAssignableFrom(i)))
            {
                var test = ((IAgent)Activator.CreateInstance(
                                                             MyType,
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
                    Log.WriteLog(Colors.BlueViolet, $"\tTrying to add {MyType.Name} {AgentModule.TryAddAgent(vtables[test], MyType)}");
                }
                else
                {
                    Log.WriteLog(Colors.BlueViolet, $"\tFound one {MyType.Name} {test.ToString("X")} but no agent");
                }
            }
        }
    }
}