using System.Runtime.InteropServices;
using Clio.Utilities;
using ff14bot.Managers;

namespace LlamaLibrary.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x98)]
public struct TripleTriadCoinExchangeCard
{
    [FieldOffset(0x0)]
    public uint ItemId;

    [FieldOffset(0x04)]
    public uint Price;

    [FieldOffset(0x88)]
    public uint Count;

    [FieldOffset(0x8C)]
    public uint Count1;

    //This is fucked - ignore
    /*[FieldOffset(0x40)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
    public byte[] name_bytes;*/

    public string Name => DataManager.GetItem(ItemId).CurrentLocaleName;
}