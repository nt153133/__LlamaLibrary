using System.Reflection;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers;

public enum ProfileType
{
    Quest,
    Duty
}

public enum DutyType
{
    Dungeon,
    Trial,
    Raid,
    Guildhest
}

public class ServerProfile
{
    public string? Name { get; set; }
    public int Level { get; set; }
    public string Quality { get; set; }
    public string Difficulty { get; set; }
    public ProfileType Type { get; set; }
    public DutyType DutyType { get; set; }
    public string? URL { get; set; }
    public ushort ZoneId { get; set; }
    public ushort DutyId { get; set; }
    public int UnlockQuest { get; set; }
    public int ItemLevel { get; set; }

    public int TrustId { get; set; }

    public string Display
    {
        get => $"[{Level}] {DataManager.InstanceContentResults[DutyId].CurrentLocaleName} {Quality}";
    }
}