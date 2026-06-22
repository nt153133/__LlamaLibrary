using System.Collections.Generic;

namespace LlamaLibrary;

/// <summary>
/// Represents a collection of memory patterns associated with a specific product (e.g., LlamaLibrary)
/// and its target game region.
/// </summary>
public class ProductPatterns
{
    /// <summary>
    /// Gets or sets the name of the product these patterns belong to.
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the game client region these patterns are intended for.
    /// Defaults to <see cref="ClientRegion.NotSpecified"/>.
    /// </summary>
    public ClientRegion ClientRegion { get; set; } = ClientRegion.NotSpecified;

    /// <summary>
    /// Gets or sets a dictionary of memory patterns, where the key is the pattern name (e.g., "VTable")
    /// and the value is the raw pattern string.
    /// </summary>
    public Dictionary<string, string> Patterns { get; set; }
}

/// <summary>
/// Defines the supported game client regions for memory pattern resolution.
/// </summary>
public enum ClientRegion
{
    /// <summary>The international/global game client.</summary>
    Global,
    /// <summary>The Chinese (Simplified) game client.</summary>
    China,
    /// <summary>The Korean game client.</summary>
    Korea,
    /// <summary>The Traditional Chinese game client.</summary>
    TraditionalChinese,
    /// <summary>The region has not been specified.</summary>
    NotSpecified
}