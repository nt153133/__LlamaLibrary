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
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Anima Weapon "Growth Fragment" (Crystal Sand) exchange interface.
    /// Facilitates the conversion of various items and currencies into Crystal Sand.
    /// </summary>
    public class AgentAWGrowthFragTrade : AgentInterface<AgentAWGrowthFragTrade>, IAgent
    {
        /// <summary>
        /// Logger for the growth fragment trade agent.
        /// </summary>
        public static LLogger Log = new LLogger("AgentAWGrowthFragTrade", Colors.Lavender);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAWGrowthFragTrade"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentAWGrowthFragTrade(IntPtr pointer) : base(pointer)
        {
        }

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentAWGrowthFragTradeOffsets.Vtable;

        /// <summary>
        /// Gets the memory pointer to the start of the exchange items array.
        /// </summary>
        public IntPtr ArrayPtr => Pointer + AgentAWGrowthFragTradeOffsets.ArrayBase;

        /// <summary>
        /// Gets the number of exchange options available in the interface.
        /// </summary>
        public int ArrayCount => Core.Memory.Read<int>(Pointer + AgentAWGrowthFragTradeOffsets.ArrayCount);

        /// <summary>
        /// Gets an array of <see cref="AnimaExchangeItemInfo"/> representing the available trade-in options.
        /// </summary>
        public AnimaExchangeItemInfo[] ExchangeItems => Core.Memory.ReadArray<AnimaExchangeItemInfo>(ArrayPtr, ArrayCount);

        /// <summary>
        /// Executes a purchase action for the specified exchange option.
        /// </summary>
        /// <param name="index">The zero-based index of the exchange item to buy.</param>
        /// <param name="qty">The quantity to purchase.</param>
        public void Buy(int index, int qty)
        {
            Core.Memory.CallInjectedWraper<int>(AgentAWGrowthFragTradeOffsets.BuyFunction, Pointer, index, qty);
        }

        /// <summary>
        /// Orchestrates the purchase of Crystal Sand using a specified currency or item.
        /// Handles opening/closing the UI, identifying the correct exchange category, and verifying the purchase outcome.
        /// </summary>
        /// <param name="itemToSpend">The item ID of the currency or item to be traded in.</param>
        /// <param name="qty">The desired quantity of Crystal Sand to purchase.</param>
        /// <param name="buyAnyAmount">If <see langword="true"/>, purchases as much as possible if the desired <paramref name="qty"/> cannot be met; otherwise, fails if the full amount is unavailable.</param>
        /// <returns><see langword="true"/> if at least some Crystal Sand was successfully purchased; otherwise <see langword="false"/>.</returns>
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