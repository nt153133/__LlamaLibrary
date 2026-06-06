using System;
using System.Runtime.InteropServices;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    /// <summary>
    /// Represents an item entry in the Grand Company Supply or Expert Delivery list.
    /// Maps to a 0xA0 byte structure in game memory used by the Grand Company Supply agent.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xA0)]
    public struct GCSupplyItem
    {
        /// <summary>
        /// Gets the memory pointer to the underlying game item object.
        /// </summary>
        [FieldOffset(0)]
        public IntPtr ItemPtr;

        /// <summary>
        /// Gets the identifier of the inventory bag containing the item.
        /// </summary>
        [FieldOffset(0x68)]
        public uint BagId;

        /// <summary>
        /// Gets the number of Grand Company seals awarded for handing in this item.
        /// </summary>
        [FieldOffset(0x78)]
        public uint Seals;

        /// <summary>
        /// Gets the raw numeric identifier of the item.
        /// </summary>
        [FieldOffset(0x84)]
        public uint ItemId;

        /// <summary>
        /// Gets the item level (iLevel) of the item.
        /// </summary>
        [FieldOffset(0x94)]
        public short ItemLevel;

        /// <summary>
        /// Gets the index of the slot within the inventory bag.
        /// </summary>
        [FieldOffset(0x97)]
        public ushort BagSlotId;

        /// <summary>
        /// Gets the type of hand-in category this item belongs to.
        /// See <see cref="LlamaLibrary.RemoteAgents.GCSupplyType"/>.
        /// </summary>
        [FieldOffset(0x99)]
        public byte HandInType;

        /// <summary>
        /// Gets a value indicating whether the item is currently part of any player gearset.
        /// </summary>
        [FieldOffset(0x9C)]
        public bool InGearSet;

        /// <summary>
        /// Gets a value indicating whether the item is located in the player's Armoury Chest.
        /// Derived from <see cref="BagId"/>.
        /// </summary>
        public bool InArmory => BagId == 3500 || (BagId - 3200) <= 9 || BagId == 3300 || BagId == 3400;

        /// <summary>
        /// Gets the <see cref="ff14bot.Managers.BagSlot"/> instance corresponding to this item.
        /// </summary>
        public BagSlot BagSlot => InventoryManager.GetBagByInventoryBagId((InventoryBagId)BagId)[BagSlotId];

        /// <summary>
        /// Gets a value indicating whether the item is High Quality (HQ).
        /// </summary>
        public bool IsHQ => BagSlot.IsHighQuality;

        /// <summary>
        /// Returns a string representation of the supply item, including name, gearset status, and quality.
        /// </summary>
        /// <returns>A formatted string describing the item.</returns>
        public override string ToString()
        {
            return $"Item: {DataManager.GetItem(ItemId).CurrentLocaleName}, {nameof(InGearSet)}: {InGearSet}, {nameof(InArmory)}: {InArmory} HQ: {IsHQ}";
        }
    }
}