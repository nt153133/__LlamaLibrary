using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ff14bot;
using ff14bot.Managers;
using LlamaBotBases.Materia;
using LlamaBotBases.Tester.Settings;
using LlamaBotBases.Tester.Tasks;
using LlamaLibrary.Extensions;
using Newtonsoft.Json;

namespace LlamaBotBases.Tester
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

            (uint, ushort) stuff = ((uint) _selectedBagSlot.BagId, _selectedBagSlot.Slot);
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

            bindingSourceInventory.Clear();

            foreach (var bagSlot in InventoryManager.EquippedItems.Where(MateriaBase.HasMateria))
            {
                bindingSourceInventory.Add(bagSlot);
            }

            itemCb.DisplayMember = "Name";
            itemCb.DataSource = bindingSourceInventory;

            if (_selectedBagSlot != null)
            {
                materiaListBox.DataSource = MateriaBase.Materia(_selectedBagSlot);
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
    }
}
