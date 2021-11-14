using ff14bot;
using ff14bot.Objects;

namespace LlamaLibrary.Extensions
{
    public static class GatheringPointObjectExtensions
    {
        public static int Base(this GatheringPointObject node)
        {
            return (int)Core.Memory.Read<uint>(node.Pointer + 0x80);
        }
    }
}