using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs
{
    /// <summary>
    /// Represents a single slot within an FC Aetherial Wheel stand.
    /// Maps the memory layout of the game's aetherial wheel data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0xC0)]
    public struct AetherWheelSlot
    {
        /// <summary>Gets the potency level (Grade 1, 2, or 3) of the aetherial wheel currently in the slot.</summary>
        [FieldOffset(0x0)]
        public ushort Grade;

        /// <summary>Gets a value indicating whether the slot is currently occupied by an aetherial wheel.</summary>
        [FieldOffset(0x2)]
        public byte InUse;

        /// <summary>Gets the number of minutes remaining until the current wheel is fully primed.</summary>
        [FieldOffset(0x6)]
        public int MinutesLeft;

        /// <summary>Gets the item ID of the aetherial wheel currently residing in the slot.</summary>
        [FieldOffset(0x8E)]
        public uint ItemId;

        /// <summary>Gets the ID of the unprimed aetherial wheel item that was originally placed in the slot.</summary>
        [FieldOffset(0xA6)]
        public uint StartingItemId;

        /// <summary>Gets the total number of minutes required for the wheel to become fully primed from a zero state.</summary>
        [FieldOffset(0xAA)]
        public uint TotalMinutes;

        /// <summary>Gets the item ID of the resulting primed aetherial wheel that will be produced.</summary>
        [FieldOffset(0xAE)]
        public uint ResultingItemId;

        /// <summary>Gets a value indicating whether the wheel has finished priming and is ready to be removed.</summary>
        [FieldOffset(0xB2)]
        public bool Primed;

        /// <summary>Gets the zero-based index of this slot within the stand.</summary>
        [FieldOffset(0xB6)]
        public byte SlotIndex;

        /// <summary>Gets the <see cref="Item"/> data for the current wheel residing in the slot.</summary>
        public Item Item => DataManager.GetItem(ItemId);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(Grade)}: {Grade}, {nameof(InUse)}: {InUse}, {nameof(MinutesLeft)}: {MinutesLeft}, {nameof(ItemId)}: {ItemId}, {nameof(StartingItemId)}: {StartingItemId}, {nameof(TotalMinutes)}: {TotalMinutes}, {nameof(ResultingItemId)}: {ResultingItemId}, {nameof(Primed)}: {Primed}, {nameof(SlotIndex)}: {SlotIndex}, {nameof(Item)}: {Item}";
        }
    }
}