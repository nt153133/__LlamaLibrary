using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopProxy : RemoteWindow<ShopProxy>
    {
        

        public ShopProxy() : base("Shop")
        {
        }

        public int ShopId => Core.Memory.Read<int>(Shop.ActiveShopPtr + ShopProxyOffsets.ShopIdPointer);
    }
}