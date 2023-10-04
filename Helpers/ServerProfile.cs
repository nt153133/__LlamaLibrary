using System.Reflection;

namespace LlamaLibrary.Helpers;

public enum ProfileType
{
    Quest,
    Duty
}

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public enum DutyType
{
    Dungeon,
    Trial,
    Raid,
    Guildhest
}

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class ServerProfile
{
    public string? Name { get; set; }
    public int Level { get; set; }
    public ProfileType Type { get; set; }
    public DutyType DutyType { get; set; }
    public string? URL { get; set; }
    public ushort ZoneId { get; set; }
    public ushort DutyId { get; set; }
    public int UnlockQuest { get; set; }
    public int ItemLevel { get; set; }
    public string Display { get => $"[{Level}] {Name}"; }
}