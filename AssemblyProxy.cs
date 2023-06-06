//!CompilerOption:AddRef:Clio.Localization.dll
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using Newtonsoft.Json.Serialization;

namespace LlamaLibrary
{
    public static class AssemblyProxy
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        private static readonly LLogger Log = new LLogger("AssemblyProxy", Colors.Bisque, LogLevel.Information);
        private static bool _initialized;
        private static object _lock = new object();
        public static void Init()
        {
            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }

                _initialized = true;
            }

            AddAssembly("Newtonsoft", typeof(JsonContract).Assembly);
            AddAssembly("GreyMagic", Core.Memory.GetType().Assembly);
            AddAssembly("ff14bot", Core.Me.GetType().Assembly);
            AddAssembly("LlamaLibrary", typeof(OffsetManager).Assembly);
            try
            {
                AddAssembly(Assembly.GetEntryAssembly()?.GetName().Name!, Assembly.GetEntryAssembly()!);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public static void AddAssembly(string name, Assembly assembly)
        {
            //Add to dictionary, make sure it's not already there
            if (Assemblies.ContainsKey(name))
            {
                return;
            }

            Assemblies.Add(name, assembly);
        }

        private static Assembly? OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (Assemblies.TryGetValue(new AssemblyName(args.Name).Name, out var resolve))
            {
                return resolve;
            }

            if (!args.Name.Contains("resources"))
            {
                Log.Debug("Assembly not found: " + args.Name + "");
            }

            return null;
        }

        public static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            switch (assemblyName.Name)
            {
                case "Newtonsoft":
                    return typeof(JsonContract).Assembly;
                case "GreyMagic":
                    return Core.Memory.GetType().Assembly;
                case "ff14bot":
                    return Core.Me.GetType().Assembly;
                case "LlamaLibrary":
                    return typeof(OffsetManager).Assembly;
                /*case "Clio.Localization":
                    return typeof(Infralution.Localization.Wpf.CultureManager).Assembly;*/
                default:
                    return null!;
            }
        }
    }
}