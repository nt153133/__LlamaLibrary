using System.Collections.Generic;

namespace LlamaLibrary;

public class ProductPatterns
{
    public string ProductName { get; set; }
    public ClientRegion ClientRegion { get; set; } = ClientRegion.Global;
    public Dictionary<string,string> Patterns { get; set; }
}

public enum ClientRegion
{
    Global,
    China,
    Korea,
    TraditionalChinese
}