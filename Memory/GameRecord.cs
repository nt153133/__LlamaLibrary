using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Memory
{
    /// <summary>
    /// Record that stores some useful information related to the different game regions, can store flags etc that need to be toggled on a per-region basis.
    /// </summary>
    /// <param name="CurrentGameVersion"></param>
    public record GameRecord(float CurrentGameVersion, OffsetFlags RegionFlag);
}
