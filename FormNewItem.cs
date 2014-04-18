using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.IO;

namespace UIPlatform
{
    public partial class FormNewItem : Form
    {
        public FormNewItem(SolutionItem parent = null)
        {
            InitializeComponent();
            if (parent == null)
            {
                radioButton3.Checked = groupBox1.Visible = true;
                radioButton1.Enabled = radioButton2.Enabled = false;
                btnAdd.Enabled = false;
            }
            else
                radioButton3.Enabled = false;
            ParentSolutionItem = parent;
        }

        public string ItemName { get; private set; }
        public string BasePath { get; private set; }
        public SolutionItem ParentSolutionItem { get; private set; }
        public SolutionItem SubSolutionItem { get; private set; }
        public UCControlItem ControlItem { get; private set; }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            SolutionItem sItem = null;
            ItemName = txtName.Text.Trim();
            if (radioButton1.Checked)
            {
                sItem = new FormSolutionItem(Guid.NewGuid().ToString(),ParentSolutionItem.Path, ItemName);

                ControlItem = UCControlItem.InitBySolutionItem(sItem,true,true);
                ControlItem.Save();
            }
            else if (radioButton2.Checked)
            {
                sItem = new ProjectSolutionItem(Guid.NewGuid().ToString(), ParentSolutionItem.Path + "\\" + ItemName, ItemName);
            }
            else if (radioButton3.Checked)
            {
                try
                {
                    if (Directory.Exists(txtPath.Text.Trim()))
                    {
                        DirectoryInfo dir = new DirectoryInfo(txtPath.Text.Trim());
                    }
                    else
                    {
                        DirectoryInfo dir = new DirectoryInfo(txtPath.Text.Trim());
                        dir.Create();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    DialogResult = DialogResult.Cancel;
                }
                BasePath = txtPath.Text.Trim();
                sItem = new RootSolutionItem(Guid.NewGuid().ToString(), ItemName, ItemName);
            }
            //sItem.Save();
            sItem.Parent = ParentSolutionItem;
            SubSolutionItem = sItem;
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = dialog.SelectedPath;
                btnAdd.Enabled = true;
            }
            else
            {
                if (string.IsNullOrEmpty(txtPath.Text.Trim()))
                {
                    btnAdd.Enabled = false;
                }
            }
        }
    }
}
