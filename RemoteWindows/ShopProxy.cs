using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopProxy : RemoteWindow<ShopProxy>
    {
        private const string WindowName = "Shop";

        private static class Offsets
        {
            [Offset("41 8B 5E ? FF 50 ? F6 05 ? ? ? ? ? Add 3 Read8")]
            internal static int ShopIdPointer;
        }

        public ShopProxy() : base(WindowName)
        {
            _name = WindowName;
        }

        public int ShopId => Core.Memory.Read<int>(Shop.ActiveShopPtr + Offsets.ShopIdPointer);
    }
}