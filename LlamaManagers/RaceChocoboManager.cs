using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using ff14bot;
using ff14bot.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.LlamaManagers;

/// <summary>
/// Static manager for accessing data related to the player's racing Chocobo.
/// Maps memory from the game's internal RaceChocoboManager to accessible properties.
/// </summary>
public static class RaceChocoboManager
{
    private static LLogger Log = new LLogger("RaceChocoboManager", Colors.Silver);

    private static IntPtr InstanceRaw => RaceChocoboManagerOffsets.Instance;

    private static  FrameCachedValue<RaceChocoboManagerStruct> _instance = new(() => Core.Memory.Read<RaceChocoboManagerStruct>(InstanceRaw));

    private static RaceChocoboManagerStruct Instance => _instance.Value;
    

    /// <summary>
    /// Gets the number of points in the Maximum Speed attribute.
    /// </summary>
    public static byte MaximumSpeed => Instance.MaximumSpeed;

    /// <summary>
    /// Gets the number of points in the Acceleration attribute.
    /// </summary>
    public static byte Acceleration => Instance.Acceleration;

    /// <summary>
    /// Gets the number of points in the Endurance attribute.
    /// </summary>
    public static byte Endurance => Instance.Endurance;

    /// <summary>
    /// Gets the number of points in the Stamina attribute.
    /// </summary>
    public static byte Stamina => Instance.Stamina;

    /// <summary>
    /// Gets the number of points in the Cunning attribute.
    /// </summary>
    public static byte Cunning => Instance.Cunning;

    /// <summary>
    /// Gets the raw bit-field containing the Chocobo's sex, weather preference, and pedigree.
    /// </summary>
    public static byte Parameters => Instance.Parameters;

    /// <summary>
    /// Gets a bit-field representing the paternal hereditary attributes and stars.
    /// </summary>
    public static short Father => Instance.Father;

    /// <summary>
    /// Gets a bit-field representing the maternal hereditary attributes and stars.
    /// </summary>
    public static short Mother => Instance.Mother;

    /// <summary>
    /// Gets the ID of the hereditary ability passed down to the Chocobo.
    /// </summary>
    public static byte AbilityHereditary => Instance.AbilityHereditary;

    /// <summary>
    /// Gets the ID of the ability learned by the Chocobo.
    /// </summary>
    public static byte AbilityLearned => Instance.AbilityLearned;

    /// <summary>
    /// Gets the ID for the first part of the Chocobo's name.
    /// </summary>
    public static short NameFirst => Instance.NameFirst;

    /// <summary>
    /// Gets the ID for the last part of the Chocobo's name.
    /// </summary>
    public static short NameLast => Instance.NameLast;

    /// <summary>
    /// Gets the current rank (level) of the racing Chocobo.
    /// </summary>
    public static byte Rank => Instance.Rank;

    /// <summary>
    /// Gets the amount of experience points earned toward the next rank.
    /// </summary>
    public static short ExperienceCurrent => Instance.ExperienceCurrent;

    /// <summary>
    /// Gets the total experience points required to reach the next rank.
    /// </summary>
    public static short ExperienceMax => Instance.ExperienceMax;

    /// <summary>
    /// Gets the ID of the Chocobo's plumage color (Stain).
    /// </summary>
    public static byte Color => Instance.Color;

    /// <summary>
    /// Gets the ID of the equipped head gear.
    /// </summary>
    public static byte GearHead => Instance.GearHead;

    /// <summary>
    /// Gets the ID of the equipped body gear.
    /// </summary>
    public static byte GearBody => Instance.GearBody;

    /// <summary>
    /// Gets the ID of the equipped leg gear.
    /// </summary>
    public static byte GearLegs => Instance.GearLegs;

    /// <summary>
    /// Gets the number of breeding or training sessions currently available.
    /// </summary>
    public static byte SessionsAvailable => Instance.SessionsAvailable;
}

/// <summary>
/// Represents the internal data structure of the game's RaceChocoboManager.
/// </summary>
/// <remarks>
/// Reference: https://github.com/aers/FFXIVClientStructs/blob/dcc9139758bf5e2ff5c0b53d73a3566eb0eec4f0/FFXIVClientStructs/FFXIV/Client/Game/RaceChocoboManager.cs
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 0x26)]
public struct RaceChocoboManagerStruct
{
    //[FieldOffset(0x00)] public int Unknownx00;
    //[FieldOffset(0x04)] public int Unknownx04;

    /// <summary>
    /// The number of points in the Maximum Speed attribute.
    /// </summary>
    /// <remarks>
    /// These aren't direct stats but represent the number of points in an attribute. To get the stat value,
    /// there is a formula that takes Pedigree &amp; Stars: <c>((Pedigree + Stars) * 0.38) * Value = Stat</c>.
    /// </remarks>
    [FieldOffset(0x08)]
    public byte MaximumSpeed;

    /// <summary>
    /// The number of points in the Acceleration attribute.
    /// </summary>
    [FieldOffset(0x09)]
    public byte Acceleration;

    /// <summary>
    /// The number of points in the Endurance attribute.
    /// </summary>
    [FieldOffset(0x0A)]
    public byte Endurance;

    /// <summary>
    /// The number of points in the Stamina attribute.
    /// </summary>
    [FieldOffset(0x0B)]
    public byte Stamina;

    /// <summary>
    /// The number of points in the Cunning attribute.
    /// </summary>
    [FieldOffset(0x0C)]
    public byte Cunning;

    /// <summary>
    /// Bit-field containing sex, weather preference, and pedigree.
    /// </summary>
    /// <remarks>
    /// On investigation this looks like:
    /// <list type="bullet">
    /// <item><description>2 bits: Sex (00:M, 01:F)</description></item>
    /// <item><description>2 bits: Weather (01:Fair, 10:Foul)</description></item>
    /// <item><description>4 bits: Pedigree</description></item>
    /// </list>
    /// </remarks>
    [FieldOffset(0x0D)]
    public byte Parameters;

    /// <summary>
    /// Bit-field representing paternal hereditary attributes and stars.
    /// </summary>
    /// <remarks>
    /// Structure:
    /// <list type="bullet">
    /// <item><description>4 bits: Pedigree</description></item>
    /// <item><description>2 bits: Unused</description></item>
    /// <item><description>5x2 bits: Cunning, Stamina, Endurance, Acceleration, Speed (Stars-1, where 3 stars = 0b10)</description></item>
    /// </list>
    /// </remarks>
    [FieldOffset(0x0E)]
    public short Father;

    /// <summary>
    /// Bit-field representing maternal hereditary attributes and stars.
    /// </summary>
    [FieldOffset(0x10)]
    public short Mother;

    /// <summary>
    /// The ID of the hereditary ability (ExcelSheet: ChocoboRaceAbility).
    /// </summary>
    [FieldOffset(0x12)]
    public byte AbilityHereditary;

    /// <summary>
    /// The ID of the learned ability (ExcelSheet: ChocoboRaceAbility).
    /// </summary>
    [FieldOffset(0x13)]
    public byte AbilityLearned;

    //[FieldOffset(0x14)] public byte Unknownx14;
    //[FieldOffset(0x15)] public byte Unknownx15;

    /// <summary>
    /// The ID for the first part of the Chocobo's name (ExcelSheet: RacingChocoboName).
    /// </summary>
    [FieldOffset(0x16)]
    public short NameFirst;

    /// <summary>
    /// The ID for the last part of the Chocobo's name (ExcelSheet: RacingChocoboName).
    /// </summary>
    [FieldOffset(0x18)]
    public short NameLast;

    /// <summary>
    /// The current rank of the racing Chocobo.
    /// </summary>
    [FieldOffset(0x1A)]
    public byte Rank;

    //[FieldOffset(0x1B)] public byte Unknownx1B;

    /// <summary>
    /// Current earned experience points.
    /// </summary>
    [FieldOffset(0x1C)]
    public short ExperienceCurrent;

    /// <summary>
    /// Required experience points for the next rank.
    /// </summary>
    [FieldOffset(0x1E)]
    public short ExperienceMax;

    /// <summary>
    /// The ID of the Chocobo's plumage color (ExcelSheet: Stain).
    /// </summary>
    [FieldOffset(0x20)]
    public byte Color;

    /// <summary>
    /// The ID of the equipped head gear (ExcelSheet: BuddyEquip).
    /// </summary>
    [FieldOffset(0x21)]
    public byte GearHead;

    /// <summary>
    /// The ID of the equipped body gear (ExcelSheet: BuddyEquip).
    /// </summary>
    [FieldOffset(0x22)]
    public byte GearBody;

    /// <summary>
    /// The ID of the equipped leg gear (ExcelSheet: BuddyEquip).
    /// </summary>
    [FieldOffset(0x23)]
    public byte GearLegs;

    /// <summary>
    /// The number of sessions currently available.
    /// </summary>
    [FieldOffset(0x24)]
    public byte SessionsAvailable;

    //[FieldOffset(0x25)] public byte Unknownx25;
}