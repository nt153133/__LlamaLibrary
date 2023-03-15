using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Managers{

public class SnipeManager
{
    private static LLogger Log = new LLogger("SnipeManager", Colors.Silver);

    internal static class Offsets
    {
        [Offset("48 8D 0D ? ? ? ? 32 DB E8 ? ? ? ? 84 C0 0F B6 CB BA ? ? ? ? 0F 45 CA 45 32 FF Add 3 TraceRelative")]
        internal static IntPtr Instance;

        [Offset("8B 83 ? ? ? ? 48 8B 54 24 ? 48 89 4D ? Add 2 Read32")]
        internal static int Id;

        //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ?
        [Offset("0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 2 Read32")]
        internal static int Active;
        
        //48 2B 8B ? ? ? ? 49 8B C7 48 F7 E9 44 8B C7 Add 3 Read32
        [Offset("48 2B 8B ? ? ? ? 49 8B C7 48 F7 E9 44 8B C7 Add 3 Read32")]
        internal static int Params;

        //66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32
        [Offset("66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32")]
        internal static int State;

        //48 8B 8B ? ? ? ? 48 8B 0C D1 Add 3 Read32
        [Offset("48 8B 8B ? ? ? ? 48 8B 0C D1 Add 3 Read32")]
        internal static int SnipeObjects;

        //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32
        [Offset("0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32")]
        internal static int Shoot;

        //0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8
        [Offset("0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
        internal static int ShootParam;

        // 44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32
        [Offset("44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32")]
        internal static int ShootData;
        
        //0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ?
        [Offset("0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
        internal static int ShootParam2;
    }

    public static IntPtr addr => Core.Memory.Read<IntPtr>(Offsets.Instance);
    public static uint SnipeRowId => Core.Memory.Read<uint>(addr + Offsets.Id);

    public static byte State
    {
        get => Core.Memory.NoCacheRead<byte>(addr + Offsets.State);
        set => Core.Memory.Write(addr + Offsets.State, value);
    }
    
    public static byte Active
    {
        get => Core.Memory.Read<byte>(addr + Offsets.Active);
        set => Core.Memory.Write(addr + Offsets.Active, value);
    }
    
    private static bool Shoot
    {
        get => Core.Memory.Read<byte>(addr + Offsets.Shoot) == 1;
        set => Core.Memory.Write(addr + Offsets.Shoot, value ? 1 : 0);
    }


    public static IntPtr SnipeObjectsPtr => Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects);
    
    public static SnipeObject[] SnipeObjects
    {
        get
        {
            var first = Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects);
            var end = Core.Memory.Read<IntPtr>(addr + Offsets.SnipeObjects + 8);

            var count = (uint)((end.ToInt64() - first.ToInt64()) / 0x48);
            Log.Information($"{count} snipe objects found");
            return Core.Memory.ReadArray<SnipeObject>(first, (int)count);
        }
    }

    public static bool Ready => State == 4;

    public static async Task Snipe(byte index)
    {
        if (addr == IntPtr.Zero) return;
        var obj = SnipeObjects[index];
        Log.Information($"Snipe {obj.GameObject?.Name ?? "Unknown Object"}");
        Core.Memory.Write<byte>(addr + Offsets.ShootData, index); //0x5000 - this should be object Index - in some cases this is obj.x4c ?
        Core.Memory.Write<byte>(addr + Offsets.ShootData + 1, Core.Memory.Read<byte>(addr + Offsets.ShootParam2)); //0x5001
        Core.Memory.Write<byte>(addr + Offsets.ShootData + 2, obj.x22); //0x5002
        Core.Memory.Write<byte>(addr + Offsets.ShootData + 4, 1); //0x5000
        State = (byte)(State + 1);

        await Coroutine.Sleep(3000);
    }
}

[StructLayout(layoutKind: LayoutKind.Explicit, Size = 0x48)]
public struct SnipeObject
{
    [FieldOffset(0)] private IntPtr objPtr;
    [FieldOffset(0x22)] internal byte x22;
    [FieldOffset(0x44)] public byte UnkByte;

    public override string ToString()
    {
        return $"{GameObject?.Name ?? "Null Object"} - byte:{UnkByte:X}";
    }

    public GameObject? GameObject
    {
        get
        {
            var id = objPtr;
            if (objPtr == IntPtr.Zero)
                return null;
            return GameObjectManager.GameObjects.FirstOrDefault(i => i.Pointer == id);
        }
    }
}
}