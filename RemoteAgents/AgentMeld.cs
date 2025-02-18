using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMeld : AgentInterface<AgentMeld>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 33 FF 48 89 03 48 8D 4B ? Add 3 TraceRelative")]

            //[OffsetCN("Search 48 8D 05 ? ? ? ? 48 8D 4B ? 48 89 03 E8 ? ? ? ? 48 8D 4B ? E8 ? ? ? ? 33 C9 Add 3 TraceRelative")]
            internal static IntPtr VTable;

            [Offset("Search 38 9F ? ? ? ? 48 8D 8D ? ? ? ? Add 2 Read32")]
            [OffsetDawntrail("Search 0F B6 9F ? ? ? ? 48 8D 8D ? ? ? ? BA ? ? ? ? 44 89 AD ? ? ? ? Add 3 Read32")]
            internal static int CanMeld;

            [Offset("Search 89 83 ? ? ? ? 48 89 83 ? ? ? ? 48 89 83 ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? Add 2 Read32")]
            internal static int ItemsToMeldCount;

            [Offset("Search 66 89 86 ? ? ? ? E8 ? ? ? ? 85 C0 Add 3 Read32")]
            internal static int IndexOfSelectedItem;

            [Offset("Search 0F BF B3 ? ? ? ? 49 8D 8F ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 0F BF BE ? ? ? ? 4D 8D 64 24 ? Add 3 Read32")]
            internal static int MateriaCount;

            [Offset("Search 48 8B 85 ? ? ? ? 48 0F BF 95 ? ? ? ? Add 3 Read32")]
            //[OffsetCN("Search 48 8B 86 ? ? ? ? 48 0F BF 96 ? ? ? ? Add 3 Read32")]
            internal static int StructStart;

            [Offset("Search 48 8B 88 ? ? ? ? 4C 8B 04 D1 Add 3 Read32")]
            internal static int ListPtr;

            [Offset("Search 89 86 ? ? ? ? 48 8B CE E8 ? ? ? ? E9 ? ? ? ? Add 2 Read32")]
            internal static int SelectedCategory;
        }

        protected AgentMeld(IntPtr pointer) : base(pointer)
        {
        }

        public bool CanMeld => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.CanMeld) == 1;

        public bool Ready => Core.Memory.NoCacheRead<byte>(LlamaLibrary.Memory.Offsets.Conditions + 7) == 0;

        public byte ItemsToMeldCount => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.ItemsToMeldCount);

        public byte IndexOfSelectedItem => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.IndexOfSelectedItem);

        public byte MateriaCount => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.MateriaCount);

        public IntPtr StructStart => Core.Memory.Read<IntPtr>(Pointer + Offsets.StructStart);

        public IntPtr ListPtr => Core.Memory.Read<IntPtr>(StructStart + Offsets.ListPtr);

        public IntPtr[] MeldList => Core.Memory.ReadArray<IntPtr>(ListPtr, ItemsToMeldCount).Select(i => Core.Memory.Read<IntPtr>(i)).ToArray();

        public MeldItem[] MeldItems => MeldList.Select(i => Core.Memory.Read<MeldItem>(i)).ToArray();

        public byte SelectedCategory => Core.Memory.NoCacheRead<byte>(Pointer + Offsets.SelectedCategory);

        public int ItemIndex(BagSlot slot)
        {
            return MeldItems.ToList().FindIndex(i => i.BagId == slot.BagId && i.BagSlot == slot.Slot);
        }
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public struct MeldItem
{
    [FieldOffset(0x0)]
    public IntPtr Pointer;

    [FieldOffset(0x8)]
    public InventoryBagId BagId;

    [FieldOffset(0xC)]
    public int BagSlot;

    [FieldOffset(0x10)]
    public uint ItemId;

    [FieldOffset(0x1C)]
    private byte _CanOvermeld;

    public bool CanOvermeld => _CanOvermeld == 1;

    [FieldOffset(0x28)]
    public ushort MateriaType1;

    [FieldOffset(0x2A)]
    public ushort MateriaType2;

    [FieldOffset(0x2C)]
    public ushort MateriaType3;

    [FieldOffset(0x2E)]
    public ushort MateriaType4;

    [FieldOffset(0x30)]
    public ushort MateriaType5;

    [FieldOffset(0x32)]
    public byte MateriaGrade1;

    [FieldOffset(0x33)]
    public byte MateriaGrade2;

    [FieldOffset(0x34)]
    public byte MateriaGrade3;

    [FieldOffset(0x35)]
    public byte MateriaGrade4;

    [FieldOffset(0x36)]
    public byte MateriaGrade5;

    public Item Item => DataManager.GetItem(ItemId);

    public int MateriaCount => new[] { MateriaType1, MateriaType2, MateriaType3, MateriaType4, MateriaType5 }.Count(i => i != 0);

    public MateriaItem[] Materia
    {
        get
        {
            var materia = new MateriaItem[MateriaCount];
            for (int i = 0; i < MateriaCount; i++)
            {
                var Type = (ushort)GetType().GetField($"MateriaType{i + 1}")?.GetValue(this);
                var Grade = (byte)GetType().GetField($"MateriaGrade{i + 1}")?.GetValue(this);
                materia[i] = ResourceManager.MateriaList.Value[Type].First(j => j.Tier == Grade);
            }

            return materia;
        }
    }

    public bool CanMeld => CanOvermeld ? Item.MateriaSlots > MateriaCount : MateriaCount < 5;

    //ToString() method
    public override string ToString()
    {
        return $"{Item?.CurrentLocaleName} {MateriaCount} {CanMeld} {string.Join(',', Materia.Select(i => i.ToString()))}";
    }
}