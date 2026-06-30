using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs;

/// <summary>
/// Represents a Triple Triad card entry in the card-to-MGP exchange interface.
/// Maps the memory layout of the exchangeable card structure.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x98)]
public struct TripleTriadCoinExchangeCard
{
    /// <summary>
    /// Gets the Item ID of the Triple Triad card.
    /// </summary>
    [FieldOffset(0x0)]
    public uint ItemId;

    /// <summary>
    /// Gets the amount of Manderville Gold Saucer Points (MGP) awarded for exchanging one card.
    /// </summary>
    [FieldOffset(0x04)]
    public uint Price;

    /// <summary>
    /// Gets the internal Card ID as defined in the Triple Triad system.
    /// </summary>
    [FieldOffset(0x08)]
    public uint CardId;

    /// <summary>
    /// Gets the current quantity of this card in the player's inventory.
    /// </summary>
    [FieldOffset(0x88)]
    public uint Count;

    /// <summary>
    /// Internal count field at offset 0x8C.
    /// </summary>
    [FieldOffset(0x8C)]
    public uint Count1;

    /// <summary>
    /// Gets the zero-based index of the card within the current display page.
    /// </summary>
    [FieldOffset(0x90)]
    public byte Index;

    /// <summary>
    /// Gets or sets the action index used when sending a selection command to the agent.
    /// Shares the same memory offset as <see cref="Index"/>.
    /// </summary>
    [FieldOffset(0x90)]
    public uint SendAction;

    //This is fucked - ignore
    /*[FieldOffset(0x40)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
    public byte[] name_bytes;*/

    /// <summary>
    /// Gets the localized name of the card retrieved from the <see cref="DataManager"/>.
    /// </summary>
    public string Name => DataManager.GetItem(ItemId).CurrentLocaleName;
}