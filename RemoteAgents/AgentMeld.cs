using System;
using System.Linq;
using System.Runtime.InteropServices;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMeld : AgentInterface<AgentMeld>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMeldOffsets.VTable;

        

        protected AgentMeld(IntPtr pointer) : base(pointer)
        {
        }

        public bool CanMeld => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.CanMeld) == 1;

        public bool Ready => Core.Memory.NoCacheRead<byte>(Offsets.Conditions + 7) == 0;

        public byte ItemsToMeldCount => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.ItemsToMeldCount);

        public byte IndexOfSelectedItem => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.IndexOfSelectedItem);

        public byte MateriaCount => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.MateriaCount);

        public IntPtr StructStart => Core.Memory.Read<IntPtr>(Pointer + AgentMeldOffsets.StructStart);

        public IntPtr ListPtr => Core.Memory.Read<IntPtr>(StructStart + AgentMeldOffsets.ListPtr);

        public IntPtr[] MeldList => Core.Memory.ReadArray<IntPtr>(ListPtr, ItemsToMeldCount).Select(i => Core.Memory.Read<IntPtr>(i)).ToArray();

        public MeldItem[] MeldItems => MeldList.Select(i => Core.Memory.Read<MeldItem>(i)).ToArray();

        public byte SelectedCategory => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.SelectedCategory);

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

    public Item? Item => DataManager.GetItem(ItemId);

    public int MateriaCount => new[] { MateriaType1, MateriaType2, MateriaType3, MateriaType4, MateriaType5 }.Count(i => i != 0);

    public MateriaItem[] Materia
    {
        get
        {
            var materia = new MateriaItem[MateriaCount];
            for (int i = 0; i < MateriaCount; i++)
            {
                var Type = (ushort)(GetType().GetField($"MateriaType{i + 1}")?.GetValue(this) ?? 0);
                var Grade = (byte)(GetType().GetField($"MateriaGrade{i + 1}")?.GetValue(this) ?? 0);
                materia[i] = ResourceManager.MateriaList.Value[Type].First(j => j.Tier == Grade);
            }

            return materia;
        }
    }

    public bool CanMeld => CanOvermeld ? Item?.MateriaSlots > MateriaCount : MateriaCount < 5;

    //ToString() method
    public override string ToString()
    {
        return $"{Item?.CurrentLocaleName} {MateriaCount} {CanMeld} {string.Join(',', Materia.Select(i => i.ToString()))}";
    }
}