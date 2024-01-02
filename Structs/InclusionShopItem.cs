using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Managers;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.Structs
{
    public class InclusionShopItem
    {
        public uint ItemId;

        public uint TrueItemId;

        public uint[] RawItemIds;

        public uint Qty;

        public List<(uint ItemId, uint Qty)> Costs;

        public byte Category;

        public byte SubCategory;

        public bool Hidden;

        public int Index;

        public InclusionShopItem(InclusionShopItemStruct shopItem, byte category, byte subCategory, int location, bool hidden)
        {
            Category = category;
            SubCategory = subCategory;

            ItemId = shopItem.ItemId;

            Qty = shopItem.SetQty;

            Hidden = hidden;

            Index = location;

            if (shopItem.ResultItemHQ)
            {
                TrueItemId = shopItem.ItemId + 1000000;
            }
            else
            {
                TrueItemId = shopItem.ItemId;
            }

            RawItemIds = new[] { shopItem.ItemId, shopItem.ItemId2 };

            Costs = new List<(uint ItemId, uint Qty)>();

            foreach (var (amount, type, flag) in shopItem.Costs)
            {
                if (amount == 0)
                {
                    continue;
                }

                switch (flag)
                {
                    case CostFlag.Item:
                        Costs.Add((type, amount));
                        break;
                    case CostFlag.HQ:
                        Costs.Add((type + 1000000, amount));
                        break;
                    case CostFlag.Tome:
                        Costs.Add((CurrencyHelper.GetTomeItemId(type), amount));
                        break;
                    case CostFlag.SpecialCurrency:
                        Costs.Add((CurrencyHelper.GetCurrencyItemId(type), amount));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ulong[] BuySendAction(int qty)
        {
            return new ulong[] { 3, 0xE, 4, (ulong)Index, 4, (ulong)qty };
        }

        public int CanAffordQty()
        {
            return Costs.Select(cost =>
                Convert.ToInt32(Math.Floor((decimal)(CurrencyHelper.GetAmountOfCurrency(cost.ItemId) / cost.Qty)))).Min();
        }

        public override string ToString()
        {
            var costs = Costs.Select(cost => $"{DataManager.GetItem(cost.ItemId).CurrentLocaleName} x {cost.Qty}");

            //costs.AddRange(Costs.Select(cost => $"{DataManager.GetItem(cost.ItemId).CurrentLocaleName} x {cost.Qty}"));
            return $"{Qty} x {DataManager.GetItem(ItemId).CurrentLocaleName} Costs: {string.Join(",", costs)}";
        }
    }
}