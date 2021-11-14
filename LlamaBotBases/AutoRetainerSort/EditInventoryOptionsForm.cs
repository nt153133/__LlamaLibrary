using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using LlamaBotBases.AutoRetainerSort.Classes;
using LlamaLibrary.Logging;
using static LlamaBotBases.AutoRetainerSort.Classes.ItemSortInfo;

namespace LlamaBotBases.AutoRetainerSort
{
    public partial class EditInventoryOptionsForm : Form
    {
        private static readonly LLogger Log = new LLogger(Strings.LogPrefix, Colors.Orange);

        public EditInventoryOptionsForm()
        {
            InitializeComponent();
        }

        public EditInventoryOptionsForm(InventorySortInfo sortInfo, int index)
        {
            InitializeComponent();
            _sortInfo = sortInfo;
            _index = index;
        }

        private readonly InventorySortInfo _sortInfo;
        private readonly int _index = int.MinValue;

        private void Form_Load(object sender, EventArgs e)
        {
            if (_sortInfo == null || _index == int.MinValue)
            {
                MessageBox.Show("Couldn't load the sorting info correctly?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            txtBoxName.Text = _sortInfo.Name;

            _bsSortTypes = new BindingSource(_sortInfo.SortTypes, "");
            _bsItems = new BindingSource(_sortInfo.SpecificItems, "");

            listBoxSortTypes.DataSource = _bsSortTypes;
            listBoxItems.DataSource = _bsItems;

            comboBoxSortType.DataSource = Enum.GetValues(typeof(SortType));

            if (Owner != null)
            {
                var ownerCenterX = Owner.Location.X + (Owner.Width / 2) - (Width / 2);
                var ownerCenterY = Owner.Location.Y + (Owner.Height / 2) - (Width / 2);
                Location = new Point(ownerCenterX, ownerCenterY);
            }
        }

        private BindingSource _bsSortTypes;

        private BindingSource _bsItems;

        private SortType NewSortType => Enum.TryParse(comboBoxSortType.SelectedValue.ToString(), out SortType sortType) ? sortType : SortType.UNKNOWN;

        private void Name_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxName.Text))
            {
                return;
            }

            _sortInfo.Name = txtBoxName.Text;
        }

        private void AddNewSortType_Click(object sender, EventArgs e)
        {
            var selected = NewSortType;
            if (_sortInfo.ContainsType(selected))
            {
                return;
            }

            foreach (var pair in AutoRetainerSortSettings.Instance.InventoryOptions)
            {
                if (pair.Key == _index)
                {
                    continue;
                }

                if (!pair.Value.ContainsType(selected))
                {
                    continue;
                }

                var dr = MessageBox.Show(
                    string.Format(Strings.AddNewItem_AlreadyExists_Warning, pair.Value.Name, selected.ToString(), pair.Value.Name, _sortInfo.Name),
                    Strings.WarningCaption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dr != DialogResult.Yes)
                {
                    return;
                }

                pair.Value.RemoveType(selected);
                break;
            }

            _bsSortTypes.Add(new SortTypeWithCount(selected));
            AutoRetainerSortSettings.Instance.Save();
        }

        private void AddNewItem_Click(object sender, EventArgs e)
        {
            var toAddIds = new HashSet<uint>();
            using (var newItemForm = new AddNewItemForm())
            {
                var dr = newItemForm.ShowDialog(this);
                if (dr != DialogResult.OK)
                {
                    return;
                }

                var selectedItem = newItemForm.SelectedSearchResult;
                if (selectedItem == null || selectedItem.RawItemId == 0)
                {
                    return;
                }

                if (newItemForm.ModifierNone)
                {
                    toAddIds.Add(selectedItem.RawItemId);
                }
                else if (newItemForm.ModifierHQ)
                {
                    toAddIds.Add(selectedItem.RawItemId + QualityOffset);
                }
                else if (newItemForm.ModifierCollectable)
                {
                    toAddIds.Add(selectedItem.RawItemId + CollectableOffset);
                }
                else
                {
                    return;
                }

                if (newItemForm.IncludeHQ)
                {
                    toAddIds.Add(selectedItem.RawItemId + QualityOffset);
                }

                if (newItemForm.IncludeCollectable)
                {
                    toAddIds.Add(selectedItem.RawItemId + CollectableOffset);
                }
            }

            foreach (var toAddId in toAddIds)
            {
                var sortInfo = ItemSortStatus.GetSortInfo(toAddId);
                var shouldAdd = true;
                foreach (var indexInfoPair in AutoRetainerSortSettings.Instance.InventoryOptions)
                {
                    if (indexInfoPair.Key == _index)
                    {
                        continue;
                    }

                    if (!indexInfoPair.Value.ContainsItem(sortInfo))
                    {
                        continue;
                    }

                    if (sortInfo.ItemInfo.Unique)
                    {
                        continue;
                    }

                    var dr = MessageBox.Show(
                        string.Format(Strings.AddNewItem_AlreadyExists_Warning, indexInfoPair.Value.Name, sortInfo.Name, indexInfoPair.Value.Name, _sortInfo.Name),
                        Strings.WarningCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dr == DialogResult.Yes)
                    {
                        indexInfoPair.Value.SpecificItems.Remove(sortInfo);
                    }
                    else
                    {
                        shouldAdd = false;
                        break;
                    }
                }

                if (!shouldAdd)
                {
                    continue;
                }

                Log.Information($"Added {sortInfo.Name} to {_sortInfo.Name}!");
                _bsItems.Add(sortInfo);
            }

            AutoRetainerSortSettings.Instance.Save();
        }

        private void DeleteSelectedSortType_Click(object sender, EventArgs e)
        {
            _bsSortTypes.Remove(listBoxSortTypes.SelectedItem);
        }

        private void DeleteSelectedItemId_Click(object sender, EventArgs e)
        {
            _bsItems.Remove(listBoxItems.SelectedItem);
        }
    }
}