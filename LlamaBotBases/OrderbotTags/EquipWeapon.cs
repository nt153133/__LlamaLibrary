using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.XmlEngine;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("EquipWeapon")]
    public class EquipWeapon : LLProfileBehavior
    {
        private bool _isDone;

        [XmlAttribute("itemIDs")]
        [XmlAttribute("ItemIDs")]
        [XmlAttribute("itemID")]
        [XmlAttribute("ItemID")]
        public int[] Item { get; set; }

        public override bool HighPriority => true;

        public EquipWeapon() : base() { }

        protected override void OnStart()
        {
        }

        protected override void OnDone()
        {
        }

        protected override void OnResetCachedDone()
        {
            _isDone = false;
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => EquipWeapons(Item));
        }

        private Task EquipWeapons(int[] weapons)
        {
            foreach (var weapon in weapons)
            {
                var itemRole = DataManager.GetItem((uint)weapon).ItemRole;
                var EquipSlot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.EquippedItems)[EquipmentSlot.MainHand];
                if (itemRole == ItemRole.Shield)
                {
                    EquipSlot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.EquippedItems)[EquipmentSlot.OffHand];
                }

                var item1 = InventoryManager.FilledInventoryAndArmory.FirstOrDefault(i => i.RawItemId == (uint)weapon);
                if (item1 != default(BagSlot))
                {
                    item1.Move(EquipSlot);
                }
            }

            _isDone = true;
            return Task.CompletedTask;
        }

        public override bool IsDone => _isDone;
    }
}