using System;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers;

public static class UIState
{
    private static readonly LLogger Log = new(nameof(UIState), Colors.Pink);

    private static class Offsets
    {
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? 48 8B 01 Add 3 TraceRelative")]
        internal static IntPtr Instance;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 66 85 D2 74 ?")]
        internal static IntPtr CardUnlocked;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 48 8B F9 0F B7 CA E8 ? ? ? ? 48 85 C0")]
        internal static IntPtr EmoteUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? 0F B6 04 08 84 D0 75 ? B8 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr MinionArray;
    }

    public static IntPtr Instance => Core.Memory.Read<IntPtr>(Offsets.Instance);

    public static bool CardUnlocked(int id) => Core.Memory.CallInjected64<bool>(Offsets.CardUnlocked, Offsets.Instance, id);

    public static bool EmoteUnlocked(int id) => Core.Memory.CallInjected64<bool>(Offsets.EmoteUnlocked, Offsets.Instance, id);

    public static byte[] MinionArray => Core.Memory.ReadBytes(Offsets.MinionArray, 0x50);

    public static bool MinionUnlocked(int id) => ((1 << (id & 7)) & MinionArray[id >> 3]) > 0;
}