using System;
using System.Collections.Generic;
using System.Diagnostics;

// Only the host surface needed by the linked wrappers. Native calls throw so
// these checks cannot accidentally claim to exercise the actual game client.
namespace ff14bot.RemoteWindows
{
    public struct TwoInt
    {
        public uint UInt;
        public bool Bool;
        public int AtkValueType;
        public string AsUTF8String;
    }

    public sealed class AtkAddonControl
    {
        public bool IsVisible = true;
        public IntPtr Pointer;
        public TwoInt[] Values = Array.Empty<TwoInt>();
        public ulong[]? LastAction;

        public void SendAction(int pairs, params ulong[] arguments)
        {
            if (arguments.Length != pairs * 2) throw new Exception("Malformed callback.");
            LastAction = arguments;
        }
    }
}

namespace LlamaLibrary.RemoteWindows
{
    public static class HostWindows
    {
        public static readonly Dictionary<string, ff14bot.RemoteWindows.AtkAddonControl> Windows = new();
    }

    public class RemoteWindow<T>
    {
        protected RemoteWindow(string name) { WindowName = name; }
        public string WindowName { get; }
        public ff14bot.RemoteWindows.AtkAddonControl? WindowByName => HostWindows.Windows.GetValueOrDefault(WindowName);
        public virtual bool IsOpen => WindowByName != null;
        protected static ff14bot.RemoteWindows.TwoInt[] ReadElements(ff14bot.RemoteWindows.AtkAddonControl window) => window.Values;
        public void Close() => WindowByName?.SendAction(1, 3, uint.MaxValue);
    }
}

namespace ff14bot
{
    public static class Core { public static readonly Memory Memory = new(); }
    public sealed class Memory
    {
        public Process Process => Process.GetCurrentProcess();
        public T Read<T>(IntPtr pointer) => throw new NotSupportedException("Native memory is not available in offline checks.");
        public T CallInjected64<T>(IntPtr pointer, params object[] arguments) => throw new NotSupportedException("Native calls are not available in offline checks.");
    }
}

namespace GreyMagic
{
    public sealed class PatternFinder : IDisposable
    {
        public PatternFinder(ff14bot.Memory memory) { }
        public IntPtr[] FindMany(string pattern, ref bool dontRebase) => throw new NotSupportedException("Native scanning is not available in offline checks.");
        public void Dispose() { }
    }
}

namespace ff14bot.Helpers
{
    public static class Logging { public static void Write(string format, params object[] arguments) { } }
}