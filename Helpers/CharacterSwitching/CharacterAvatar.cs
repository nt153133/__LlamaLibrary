using System;
using LlamaLibrary.Enums;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Identifies an FFXIV character by display name, home world, data centre, and unique character id.
/// </summary>
/// <remarks>
/// Instances are JSON-serializable and are used throughout the character-switching subsystem to
/// target a specific character when automating multi-character workflows. The <see cref="Censored"/>
/// flag controls whether the character's real name is shown in logs and UI.
/// </remarks>
[Serializable]
public class CharacterAvatar
{
    /// <summary>
    /// Gets or sets the character's in-game display name (first and last name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data centre group that the character's home world belongs to.
    /// </summary>
    public WorldDCGroupType DC { get; set; } = WorldDCGroupType.Invalid;

    /// <summary>
    /// Gets or sets the character's home world (server).
    /// </summary>
    public World HomeWorld { get; set; } = World.SetMe;

    /// <summary>
    /// Gets or sets the unique numeric identifier for the character (Lodestone / game character id).
    /// </summary>
    public ulong CharacterId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the character's real name should be hidden in logs and UI.
    /// </summary>
    /// <value><c>true</c> to display only initials or a truncated name; <c>false</c> to display the full name.</value>
    public bool Censored { get; set; } = false;

    /// <summary>
    /// Gets the full display string in the format <c>Name @ Server</c>.
    /// </summary>
    [JsonIgnore]
    public string FullName => $"{Name} @ {Server}";

    /// <summary>
    /// Gets a privacy-safe version of the character name.
    /// </summary>
    /// <remarks>
    /// For names containing a space (first and last name), returns the two initials separated by a space (e.g., <c>J S</c>).
    /// For single-segment names longer than two characters, returns the first two characters followed by an ellipsis.
    /// Otherwise returns the name unchanged.
    /// </remarks>
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

    /// <summary>
    /// Gets the name to display in UI and logs, respecting the <see cref="Censored"/> flag.
    /// </summary>
    /// <value><see cref="CensoredName"/> when <see cref="Censored"/> is <c>true</c>; otherwise <see cref="Name"/>.</value>
    [JsonIgnore]
    public string DisplayName => Censored ? CensoredName : Name;

    /// <summary>
    /// Gets the home-world name as a trimmed string, derived from <see cref="HomeWorld"/>.
    /// </summary>
    [JsonIgnore]
    public string Server => HomeWorld.WorldName().Trim().Trim('\u0000');

    /// <summary>
    /// Returns a string representation of the character in the format <c>Name @ Server</c>.
    /// </summary>
    /// <returns>A human-readable identifier for the character.</returns>
    public override string ToString()
    {
        return $"{Name} @ {Server}";
    }
}