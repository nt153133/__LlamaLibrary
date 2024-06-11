using System;
using ff14bot;
using GreyMagic;

namespace LlamaLibrary.Memory.PatternFinders;

public class GreyMagicPf : PatternFinder, ISearcher
{
    private readonly DebounceDispatcher? _debounceDispatcher;

    public GreyMagicPf(MemoryBase memory, int sizeOfCode) : base(memory, sizeOfCode)
    {
        ImageBase = memory.ImageBase;
    }

    public GreyMagicPf(MemoryBase memory) : base(memory)
    {
        ImageBase = memory.ImageBase;
    }

    public GreyMagicPf() : base(Core.Memory)
    {
        ImageBase = Core.Memory.ImageBase;
    }

    public GreyMagicPf(DebounceDispatcher debounceDispatcher) : base(Core.Memory)
    {
        ImageBase = Core.Memory.ImageBase;
        _debounceDispatcher = debounceDispatcher;
    }

    public IntPtr ImageBase { get; }

    public IntPtr FindSingle(string pattern)
    {
        _debounceDispatcher?.Debounce(30_000);
        return FindSingle(pattern, true);
    }

    public IntPtr[] SearchMany(string pattern)
    {
        var dontRebase = false;
        _debounceDispatcher?.Debounce(30_000);
        return FindMany(pattern, ref dontRebase);
    }
}