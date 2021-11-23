
namespace LlamaBotBases.Tester
{
    partial class Utilities
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnHuntStart = new System.Windows.Forms.Button();
            this.pgHunts = new System.Windows.Forms.PropertyGrid();
            this.tabMateria = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRemoveMateria = new System.Windows.Forms.Button();
            this.materiaListBox = new System.Windows.Forms.ListBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.itemCb = new System.Windows.Forms.ComboBox();
            this.bindingSourceInventory = new System.Windows.Forms.BindingSource(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabMateria.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.bindingSourceInventory)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabMateria);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(393, 388);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnHuntStart);
            this.tabPage1.Controls.Add(this.pgHunts);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(385, 362);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Daily Hunts";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnHuntStart
            // 
            this.btnHuntStart.Location = new System.Drawing.Point(265, 321);
            this.btnHuntStart.Name = "btnHuntStart";
            this.btnHuntStart.Size = new System.Drawing.Size(95, 25);
            this.btnHuntStart.TabIndex = 1;
            this.btnHuntStart.Text = "Start";
            this.btnHuntStart.UseVisualStyleBackColor = true;
            this.btnHuntStart.Click += new System.EventHandler(this.btnHuntStart_Click);
            // 
            // pgHunts
            // 
            this.pgHunts.Location = new System.Drawing.Point(9, 12);
            this.pgHunts.Name = "pgHunts";
            this.pgHunts.Size = new System.Drawing.Size(352, 238);
            this.pgHunts.TabIndex = 0;
            // 
            // tabMateria
            // 
            this.tabMateria.Controls.Add(this.groupBox1);
            this.tabMateria.Location = new System.Drawing.Point(4, 22);
            this.tabMateria.Name = "tabMateria";
            this.tabMateria.Padding = new System.Windows.Forms.Padding(3);
            this.tabMateria.Size = new System.Drawing.Size(385, 362);
            this.tabMateria.TabIndex = 1;
            this.tabMateria.Text = "Materia";
            this.tabMateria.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRemoveMateria);
            this.groupBox1.Controls.Add(this.materiaListBox);
            this.groupBox1.Controls.Add(this.btnRefresh);
            this.groupBox1.Controls.Add(this.itemCb);
            this.groupBox1.Location = new System.Drawing.Point(19, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(334, 263);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remove Materia";
            // 
            // btnRemoveMateria
            // 
            this.btnRemoveMateria.Location = new System.Drawing.Point(9, 185);
            this.btnRemoveMateria.Name = "btnRemoveMateria";
            this.btnRemoveMateria.Size = new System.Drawing.Size(142, 25);
            this.btnRemoveMateria.TabIndex = 3;
            this.btnRemoveMateria.Text = "Remove All Materia";
            this.btnRemoveMateria.UseVisualStyleBackColor = true;
            this.btnRemoveMateria.Click += new System.EventHandler(this.btnRemoveMateria_Click);
            // 
            // materiaListBox
            // 
            this.materiaListBox.FormattingEnabled = true;
            this.materiaListBox.Location = new System.Drawing.Point(9, 45);
            this.materiaListBox.Name = "materiaListBox";
            this.materiaListBox.Size = new System.Drawing.Size(297, 134);
            this.materiaListBox.TabIndex = 2;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(229, 17);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(78, 21);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // itemCb
            // 
            this.itemCb.FormattingEnabled = true;
            this.itemCb.Location = new System.Drawing.Point(9, 17);
            this.itemCb.Name = "itemCb";
            this.itemCb.Size = new System.Drawing.Size(209, 21);
            this.itemCb.TabIndex = 0;
            // 
            // bindingSourceInventory
            // 
            this.bindingSourceInventory.CurrentChanged += new System.EventHandler(this.bindingSourceInventory_CurrentChanged);
            // 
            // Utilities
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "Utilities";
            this.Text = "Utilities";
            this.Load += new System.EventHandler(this.Utilities_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabMateria.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.bindingSourceInventory)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.PropertyGrid pgHunts;
        private System.Windows.Forms.Button btnHuntStart;

        private System.Windows.Forms.BindingSource bindingSourceInventory;

        private System.Windows.Forms.BindingSource bindingSource1;

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabMateria;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox itemCb;
        private System.Windows.Forms.ListBox materiaListBox;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnRemoveMateria;
    }
}