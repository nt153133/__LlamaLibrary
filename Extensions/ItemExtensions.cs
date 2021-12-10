using ff14bot.Managers;
using LlamaLibrary.Enums;

namespace LlamaLibrary.Extensions
{
    public static class ItemExtensions
    {
        public static MyItemRole MyItemRole(this Item item)
        {
            return (MyItemRole)((byte)item.ItemRole);
        }

        public static string LocaleName(this Item item)
        {
            return item.IsHighQuality ? $"{item.CurrentLocaleName} (HQ)" : item.CurrentLocaleName;
        }
    }
}