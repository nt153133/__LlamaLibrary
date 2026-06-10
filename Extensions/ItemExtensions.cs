using ff14bot.Managers;
using LlamaLibrary.Enums;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="Item"/> class.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Gets the internal item role for this item, cast to the <see cref="MyItemRole"/> enum.
        /// </summary>
        /// <param name="item">The item instance.</param>
        /// <returns>The <see cref="MyItemRole"/> value for the item.</returns>
        public static MyItemRole MyItemRole(this Item item)
        {
            return (MyItemRole)(byte)item.ItemRole;
        }

        /// <summary>
        /// Gets the localized name of the item, appending "(HQ)" if the item is High Quality.
        /// </summary>
        /// <param name="item">The item instance.</param>
        /// <returns>The localized name string.</returns>
        public static string LocaleName(this Item item)
        {
            return item.IsHighQuality ? $"{item.CurrentLocaleName} (HQ)" : item.CurrentLocaleName;
        }
    }
}