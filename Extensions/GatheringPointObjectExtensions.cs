using ff14bot;
using ff14bot.Objects;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="GatheringPointObject"/> class.
    /// </summary>
    public static class GatheringPointObjectExtensions
    {
        /// <summary>
        /// Retrieves the internal base identifier for the specified gathering node by reading from memory.
        /// </summary>
        /// <param name="node">The gathering point object instance.</param>
        /// <returns>The numeric base ID of the node.</returns>
        public static int Base(this GatheringPointObject node)
        {
            return (int)Core.Memory.Read<uint>(node.Pointer + 0x80);
        }
    }
}