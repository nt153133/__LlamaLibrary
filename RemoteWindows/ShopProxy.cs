using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopProxy : RemoteWindow<ShopProxy>
    {
        private static class Offsets
        {
            [Offset("Search 41 8B 5E ? FF 50 ? F6 05 ? ? ? ? ? Add 3 Read8")]
            internal static int ShopIdPointer;
        }

        public ShopProxy() : base("Shop")
        {
        }

        public int ShopId => Core.Memory.Read<int>(Shop.ActiveShopPtr + Offsets.ShopIdPointer);
    }
}