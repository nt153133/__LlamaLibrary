namespace LlamaLibrary.Structs;

public class BeastTribeExd
{
    public byte MaxRank { get; private set; }

    public byte Expansion { get; private set; }

    public ushort Currency { get; private set; }

    public string Name { get; set; }

    public BeastTribeExd(BeastTribeExdTemp temp, string name)
    {
        MaxRank = temp.MaxRank;
        Expansion = temp.Expansion;
        Currency = temp.Currency;
        Name = name;
    }
}