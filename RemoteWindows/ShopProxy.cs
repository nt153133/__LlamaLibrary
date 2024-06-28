using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopProxy : RemoteWindow<ShopProxy>
    {
        private static class Offsets
        {
            //6.3
            [Offset("Search 8B 5E ? FF 50 ? F6 05 ? ? ? ? ? Add 2 Read8")]
            [OffsetDawntrail("Search 8B 5B ? FF 50 08 F6 05 ? ? ? ? ? 48 89 44 24 ? C7 44 24 ? ? ? ? ? 48 C7 44 24 ? ? ? ? ? 89 5C 24 68 0F 85 ? ? ? ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B C8 Add 2 Read8")]
            // pre6.3 [OffsetCN("Search 41 8B 5E ? FF 50 ? F6 05 ? ? ? ? ? Add 3 Read8")]
            internal static int ShopIdPointer;
        }

        public ShopProxy() : base("Shop")
        {
        }

        public int ShopId => Core.Memory.Read<int>(Shop.ActiveShopPtr + Offsets.ShopIdPointer);
    }
}