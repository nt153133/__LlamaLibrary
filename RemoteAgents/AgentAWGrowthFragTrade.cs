using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Buddy.Offsets;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentAWGrowthFragTrade : AgentInterface<AgentAWGrowthFragTrade>, IAgent
    {
        public static LLogger Log = new LLogger("AgentAWGrowthFragTrade", Colors.Lavender);

        

        protected AgentAWGrowthFragTrade(IntPtr pointer) : base(pointer)
        {
        }

        public int RegisteredAgentId => PublicOffsets.LLamaAgentIds.AgentAWGrowthFragTrade;

        public IntPtr ArrayPtr => Pointer + AgentAWGrowthFragTradeOffsets.ArrayBase;

        public int ArrayCount => Core.Memory.Read<int>(Pointer + AgentAWGrowthFragTradeOffsets.ArrayCount);

        public AnimaExchangeItemInfo[] ExchangeItems => Core.Memory.ReadArray<AnimaExchangeItemInfo>(ArrayPtr, ArrayCount);

        public void Buy(int index, int qty)
        {
            Core.Memory.CallInjectedWraper<int>(AgentAWGrowthFragTradeOffsets.BuyFunction, Pointer, index, qty);
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