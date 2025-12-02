using System;
using LlamaLibrary.Enums;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.CharacterSwitching;

[Serializable]
public class CharacterAvatar
{
    public string Name { get; set; } = string.Empty;
    public WorldDCGroupType DC { get; set; } = WorldDCGroupType.Invalid;
    public World HomeWorld { get; set; } = World.SetMe;
    public ulong CharacterId { get; set; }
    public bool Censored { get; set; } = false;

    [JsonIgnore]
    public string FullName => $"{Name} @ {Server}";

    [JsonIgnore]
    public string CensoredName {
        get
        {
            if (Name.Contains(' '))
            {
                //return initals
                var names = Name.Split(' ');
                return $"{names[0][0]} {names[1][0]}";
            }
            else if (Name.Length > 2)
            {
                return $"{Name[0]}{Name[1]}...";
            }
            else
            {
                return Name;
            }
        }}

    [JsonIgnore]
    public string DisplayName => Censored ? CensoredName : Name;

    [JsonIgnore]
    public string Server => HomeWorld.WorldName().Trim().Trim('\u0000');

    public override string ToString()
    {
        return $"{Name} @ {Server}";
    }
}