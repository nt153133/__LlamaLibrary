using System;
using System.Reflection;
using ff14bot;
using LlamaLibrary.Memory;
using Newtonsoft.Json.Serialization;

namespace LlamaLibrary
{
    public static class AssemblyProxy
    {
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
                case "Clio.Localization":
                    return typeof(Infralution.Localization.Wpf.CultureManager).Assembly;
                default:
                    return null!;
            }
        }
    }
}