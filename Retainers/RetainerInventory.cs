using System.Collections.Generic;
using System.Windows.Media;
using ff14bot.Managers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Retainers
{
    //TODO Chopping block, Tuck's botbase handles retainer inventories now
    public class RetainerInventory
    {
        private static readonly LLogger Log = new(nameof(RetainerInventory), Colors.White);

        private readonly IDictionary<uint, BagSlot> dict = new Dictionary<uint, BagSlot>();

        public void AddItem(BagSlot slot)
        {
            if (HasItem(slot.TrueItemId))
            {
                Log.Error($"Trying to add item twice \t Name: {slot.Item.CurrentLocaleName} Count: {slot.Count} BagId: {slot.BagId} IsHQ: {slot.Item.IsHighQuality}");
                return;
            }

            dict.Add(slot.TrueItemId, slot);
        }

        public BagSlot GetItem(uint trueItemId)
        {
            return dict.TryGetValue(trueItemId, out var returnBagSlot) ? returnBagSlot : null;
        }

        public bool HasItem(uint trueItemId)
        {
            return dict.ContainsKey(trueItemId);
        }

        public void PrintList()
        {
            foreach (var slot in dict)
            {
                var item = slot.Value;
                Log.Information($"Name: {item.Item.CurrentLocaleName} Count: {item.Count} RawId: {item.RawItemId} IsHQ: {item.Item.IsHighQuality} TrueID: {item.TrueItemId}");
            }
        }
    }
}