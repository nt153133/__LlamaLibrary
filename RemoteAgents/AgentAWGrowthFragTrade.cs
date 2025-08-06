using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAWGrowthFragTrade : AgentInterface<AgentAWGrowthFragTrade>, IAgent
    {
        public static LLogger Log = new LLogger("AgentAWGrowthFragTrade", Colors.Lavender);

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 8D 4B ? 48 89 03 33 D2 Add 3 TraceRelative")]
            internal static IntPtr Vtable;

            //(AgentPtr, index, qty)
            [Offset("Search 4C 8B DC 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 83 B9 ? ? ? ? ?")]
            [OffsetCN("Search 4C 8B DC 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 44 24 ? 48 83 B9 ? ? ? ? ? 41 8B E8 48 63 FA")]
            internal static IntPtr BuyFunction;

            [Offset("Search 49 8D 4D ? 4C 8D 0D ? ? ? ? Add 3 Read8")]
            internal static int ArrayBase;

            [Offset("Search 45 89 BD ? ? ? ? 49 8D 4D ? Add 3 Read32")]
            internal static int ArrayCount;
        }

        protected AgentAWGrowthFragTrade(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr RegisteredVtable => Offsets.Vtable;

        public IntPtr ArrayPtr => Pointer + Offsets.ArrayBase;

        public int ArrayCount => Core.Memory.Read<int>(Pointer + Offsets.ArrayCount);

        public AnimaExchangeItemInfo[] ExchangeItems => Core.Memory.ReadArray<AnimaExchangeItemInfo>(ArrayPtr, ArrayCount);

        public void Buy(int index, int qty)
        {
            Core.Memory.CallInjectedWraper<int>(Offsets.BuyFunction, Pointer, index, qty);
        }

        public static async Task<bool> BuyCrystalSand(uint itemToSpend, int qty, bool buyAnyAmount = false)
        {
            if (!await AWGrowthFragTrade.OpenExchangeWindow())
            {
                await AWGrowthFragTrade.CloseExchangeWindow();
                Log.Error("Failed to open exchange window");
                return false;
            }

            var items = Instance.ExchangeItems;

            var category = items.Where(i => i.RequiredItems.Any(animaExchangeRequiredItem => animaExchangeRequiredItem.ItemId == itemToSpend)).ToList();

            if (category.Count == 0)
            {
                Log.Error($"No exchanges found that use {DataManager.GetItem(itemToSpend)?.CurrentLocaleName}");
                await AWGrowthFragTrade.CloseExchangeWindow();
                return false;
            }

            var exchange = category.First();

            var maxAfford = exchange.RequiredItems.Min(i => i.CanAffordAmount) * exchange.ResultingItemQuantity;

            if ((maxAfford < qty) && !buyAnyAmount)
            {
                Log.Error($"Not enough {DataManager.GetItem(itemToSpend)?.CurrentLocaleName} to buy {qty} {exchange.ResultingItem.CurrentLocaleName}");
                await AWGrowthFragTrade.CloseExchangeWindow();
                return false;
            }

            var amountToBuy = buyAnyAmount ? Math.Min(qty, maxAfford) : qty;
            amountToBuy = (int)(amountToBuy / exchange.ResultingItemQuantity);

            var startingAmount = ConditionParser.ItemCount(exchange.ResultingItemId);

            Instance.Buy(exchange.Index, (int)amountToBuy);

            await Coroutine.Wait(10000, () => ConditionParser.ItemCount(exchange.ResultingItemId) > startingAmount);

            await Coroutine.Wait(10000, () => AWGrowthFragTrade.Instance.IsOpen);

            await AWGrowthFragTrade.CloseExchangeWindow();

            return ConditionParser.ItemCount(exchange.ResultingItemId) > startingAmount;
        }
    }
}