using System;
using System.Linq;
using System.Windows.Forms;
using ff14bot;
using ff14bot.Managers;
using LlamaBotBases.LlamaUtilities.Settings;
using LlamaBotBases.LlamaUtilities.Tasks;
using LlamaLibrary.Extensions;
using Newtonsoft.Json;

namespace LlamaBotBases.LlamaUtilities
{
    public partial class Utilities : Form
    {
        private BagSlot _selectedBagSlot;

        public Utilities()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            bindingSourceInventory.Clear();
            foreach (var bagSlot in InventoryManager.EquippedItems.Where(i => i.HasMateria()))
            {
                bindingSourceInventory.Add(bagSlot);
            }

            itemCb.DisplayMember = "Name";
            itemCb.DataSource = bindingSourceInventory;
            itemCb.Update();
            itemCb_SelectionChangeCommitted(this, null);

            if (_selectedBagSlot != null)
            {
                materiaListBox.DataSource = _selectedBagSlot.Materia();
                materiaListBox.DisplayMember = "ItemName";
            }
        }

        private void btnRemoveMateria_Click(object sender, EventArgs e)
        {
            if (!_selectedBagSlot.IsValid || !_selectedBagSlot.IsFilled || !_selectedBagSlot.HasMateria())
            {
                return;
            }

            (uint, ushort) stuff = ((uint)_selectedBagSlot.BagId, _selectedBagSlot.Slot);
            var taskInfo = JsonConvert.SerializeObject(stuff);

            var task = new BotTask()
            {
                Type = TaskType.MateriaRemove,
                TaskInfo = taskInfo
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();

        }

        private static void StartBotBase()
        {
            if (BotManager.Current.Name == UtilitiesBase._name)
            {
                BotManager.Current.Start();
                TreeRoot.Start();
            }
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                bindingSourceInventory.Clear();
                foreach (var bagSlot in InventoryManager.EquippedItems.Where(i => i.HasMateria()))
                {
                    bindingSourceInventory.Add(bagSlot);
                }

                itemCb.DisplayMember = "Name";
                itemCb.DataSource = bindingSourceInventory;

                if (_selectedBagSlot != null)
                {
                    materiaListBox.DataSource = _selectedBagSlot.Materia();
                    materiaListBox.DisplayMember = "ItemName";
                }
            }
        }

        private void Utilities_Load(object sender, EventArgs e)
        {
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            itemCb.SelectionChangeCommitted += new System.EventHandler(itemCb_SelectionChangeCommitted);
            pgHunts.SelectedObject = HuntsSettings.Instance;
            pgRetainers.SelectedObject = RetainerSettings.Instance;

            bindingSourceInventory.Clear();

            foreach (var bagSlot in InventoryManager.EquippedItems.Where(i => i.HasMateria()))
            {
                bindingSourceInventory.Add(bagSlot);
            }

            itemCb.DisplayMember = "Name";
            itemCb.DataSource = bindingSourceInventory;

            if (_selectedBagSlot != null)
            {
                materiaListBox.DataSource = _selectedBagSlot.Materia();
                materiaListBox.DisplayMember = "ItemName";
            }
        }

        private void itemCb_SelectionChangeCommitted(object sender, EventArgs e)
        {
            _selectedBagSlot = (BagSlot)itemCb.SelectedItem;
            materiaListBox.DataSource = _selectedBagSlot.Materia();
            materiaListBox.DisplayMember = "ItemName";
        }

        private void bindingSourceInventory_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void btnHuntStart_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Hunts,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnReduce_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Reduce,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Extract,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnCoffers_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Coffers,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnHousing_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Housing,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnCustomDeliveries_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.CustomDeliveries,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnGcTurin_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.GcTurnin,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnRetainers_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Retainers,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnFCWorkshop_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.FCWorkshop,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }

        private void btnDesynth_Click(object sender, EventArgs e)
        {
            var task = new BotTask()
            {
                Type = TaskType.Desynth,
                TaskInfo = ""
            };

            UtilitiesBase.BotTask = task;
            StartBotBase();
        }
    }
}