using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.Managers;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Helpers
{
    public static class InventoryHelpers
    {
        private static readonly string Name = "InventoryHelpers";
        private static readonly Color LogColor = Colors.White;
        private static readonly LLogger Log = new LLogger(Name, LogColor);
        public static async Task LowerQualityAndCombine(int itemId)
        {
            var HQslots = InventoryManager.FilledSlots.Where(slot => slot.RawItemId == itemId && slot.IsHighQuality);

            if (HQslots.Any())
            {
                HQslots.First().LowerQuality();
                await Coroutine.Sleep(1000);
            }

            var NQslots = InventoryManager.FilledSlots.Where(slot => slot.RawItemId == itemId && !slot.IsHighQuality);

            if (NQslots.Count() > 1)
            {
                var firstSlot = NQslots.First();
                foreach (var slot in NQslots.Skip(1))
                {
                    slot.Move(firstSlot);
                    await Coroutine.Sleep(500);
                }
            }
        }
    }
}