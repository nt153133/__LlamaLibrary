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
    /// <summary>
    /// Remote agent for the Materia Melding interface.
    /// Manages the process of affixing materia to equipment, including tracking eligible items and current materia status.
    /// </summary>
    public class AgentMeld : AgentInterface<AgentMeld>, IAgent
    {
        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentMeldOffsets.VTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMeld"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentMeld(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the player is currently able to meld (e.g., has the required level/quest).
        /// </summary>
        public bool CanMeld => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.CanMeld) == 1;

        /// <summary>
        /// Gets a value indicating whether the meld interface is ready for the next action.
        /// </summary>
        public bool Ready => Core.Memory.NoCacheRead<byte>(Offsets.Conditions + 7) == 0;

        /// <summary>
        /// Gets the total number of items eligible for melding in the currently selected inventory category.
        /// </summary>
        public byte ItemsToMeldCount => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.ItemsToMeldCount);

        /// <summary>
        /// Gets the index of the currently selected item within the melding list.
        /// </summary>
        public byte IndexOfSelectedItem => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.IndexOfSelectedItem);

        /// <summary>
        /// Gets the number of materia pieces affixed to the currently selected item.
        /// </summary>
        public byte MateriaCount => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.MateriaCount);

        /// <summary>
        /// Gets the memory pointer to the start of the melding data structure.
        /// </summary>
        public IntPtr StructStart => Core.Memory.Read<IntPtr>(Pointer + AgentMeldOffsets.StructStart);

        /// <summary>
        /// Gets the memory pointer to the list of items available for melding.
        /// </summary>
        public IntPtr ListPtr => Core.Memory.Read<IntPtr>(StructStart + AgentMeldOffsets.ListPtr);

        /// <summary>
        /// Gets an array of memory pointers to the individual items in the melding list.
        /// </summary>
        public IntPtr[] MeldList => Core.Memory.ReadArray<IntPtr>(ListPtr, ItemsToMeldCount).Select(i => Core.Memory.Read<IntPtr>(i)).ToArray();

        /// <summary>
        /// Gets an array of <see cref="MeldItem"/> structures for all items currently in the melding list.
        /// </summary>
        public MeldItem[] MeldItems => MeldList.Select(i => Core.Memory.Read<MeldItem>(i)).ToArray();

        /// <summary>
        /// Gets the identifier for the currently selected inventory category (e.g., Equipment, Armoury Chest).
        /// </summary>
        public byte SelectedCategory => Core.Memory.NoCacheRead<byte>(Pointer + AgentMeldOffsets.SelectedCategory);

        /// <summary>
        /// Finds the index of a specific <see cref="BagSlot"/> within the melding list.
        /// </summary>
        /// <param name="slot">The bag slot to find.</param>
        /// <returns>The zero-based index of the item, or -1 if not found.</returns>
        public int ItemIndex(BagSlot slot)
        {
            return MeldItems.ToList().FindIndex(i => i.BagId == slot.BagId && i.BagSlot == slot.Slot);
        }
    }
}

/// <summary>
/// Represents an item and its current materia state within the melding interface.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public struct MeldItem
{
    /// <summary>The memory address of this item's data.</summary>
    [FieldOffset(0x0)]
    public IntPtr Pointer;

    /// <summary>The inventory bag containing this item.</summary>
    [FieldOffset(0x8)]
    public InventoryBagId BagId;

    /// <summary>The slot index within the inventory bag.</summary>
    [FieldOffset(0xC)]
    public ushort BagSlot;

    /// <summary>The unique item identifier.</summary>
    [FieldOffset(0x10)]
    public uint ItemId;

    [FieldOffset(0x1C)]
    private byte _CanOvermeld;

    /// <summary>Gets a value indicating whether this item can have more materia affixed than it has slots for (Advanced Melding).</summary>
    public bool CanOvermeld => _CanOvermeld == 1;

    /// <summary>The type identifier for the first materia slot.</summary>
    [FieldOffset(0x28)]
    public ushort MateriaType1;

    /// <summary>The type identifier for the second materia slot.</summary>
    [FieldOffset(0x2A)]
    public ushort MateriaType2;

    /// <summary>The type identifier for the third materia slot.</summary>
    [FieldOffset(0x2C)]
    public ushort MateriaType3;

    /// <summary>The type identifier for the fourth materia slot.</summary>
    [FieldOffset(0x2E)]
    public ushort MateriaType4;

    /// <summary>The type identifier for the fifth materia slot.</summary>
    [FieldOffset(0x30)]
    public ushort MateriaType5;

    /// <summary>The grade (tier) of the materia in the first slot.</summary>
    [FieldOffset(0x32)]
    public byte MateriaGrade1;

    /// <summary>The grade (tier) of the materia in the second slot.</summary>
    [FieldOffset(0x33)]
    public byte MateriaGrade2;

    /// <summary>The grade (tier) of the materia in the third slot.</summary>
    [FieldOffset(0x34)]
    public byte MateriaGrade3;

    /// <summary>The grade (tier) of the materia in the fourth slot.</summary>
    [FieldOffset(0x35)]
    public byte MateriaGrade4;

    /// <summary>The grade (tier) of the materia in the fifth slot.</summary>
    [FieldOffset(0x36)]
    public byte MateriaGrade5;

    /// <summary>Gets the item metadata from the game's data manager.</summary>
    public Item? Item => DataManager.GetItem(ItemId);

    /// <summary>Gets the total number of materia pieces currently affixed to this item.</summary>
    public int MateriaCount => new[] { MateriaType1, MateriaType2, MateriaType3, MateriaType4, MateriaType5 }.Count(i => i != 0);

    /// <summary>Gets an array of <see cref="MateriaItem"/> objects representing the materia currently affixed to this item.</summary>
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

    /// <summary>Gets a value indicating whether more materia can be affixed to this item.</summary>
    public bool CanMeld => CanOvermeld ?  MateriaCount < 5 : Item?.MateriaSlots > MateriaCount;

    /// <summary>Returns a string representation of the item and its materia state.</summary>
    /// <returns>A string containing the item name, materia count, and details of each affixed materia.</returns>
    public override string ToString()
    {
        return $"{Item?.CurrentLocaleName} {MateriaCount} {CanMeld} {string.Join(',', Materia.Select(i => i.ToString()))}";
    }
}