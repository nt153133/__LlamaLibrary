using System.Collections.Generic;

namespace LlamaLibrary;

/// <summary>
/// Represents a collection of memory search patterns and associated metadata for a specific game client product or region.
/// </summary>
public class ProductPatterns
{
    /// <summary>
    /// Gets or sets the display name of the product or library (e.g., "LlamaLibrary").
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the geographic or version-specific region of the game client.
    /// Defaults to <see cref="ClientRegion.NotSpecified"/>.
    /// </summary>
    public ClientRegion ClientRegion { get; set; } = ClientRegion.NotSpecified;

    /// <summary>
    /// Gets or sets the dictionary of memory patterns, where the key is a unique identifier (e.g., "VTable")
    /// and the value is the raw pattern string used for scanning.
    /// </summary>
    public Dictionary<string, string> Patterns { get; set; }
}

/// <summary>
/// Specifies the geographic region or distribution of the Final Fantasy XIV game client.
/// Used to differentiate between different binary structures and memory offsets.
/// </summary>
public enum ClientRegion
{
    /// <summary>The international version of the game client.</summary>
    Global,

    /// <summary>The Chinese (Shengqu Games) version of the game client.</summary>
    China,

    /// <summary>The Korean (Actoz Soft) version of the game client.</summary>
    Korea,

    /// <summary>The Traditional Chinese version of the game client.</summary>
    TraditionalChinese,

    /// <summary>Region not specified or unknown.</summary>
    NotSpecified
}