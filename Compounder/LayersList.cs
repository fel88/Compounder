using Compounder.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compounder
{
    public partial class LayersList : UserControl
    {
        public LayersList()
        {
            InitializeComponent();
        }
        IEditor Editor;
        public void Init(IEditor editor)
        {
            Editor = editor;
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in Editor.Project.Layers)
            {
                var add = Editor.ActiveLayer == item ? "(active)" : "";
                listView1.Items.Add(new ListViewItem(new string[] {
                    $"{item.Name} {add}",
                    item.Visible.ToString()
                })
                { Tag = item });
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Project.Layers.Add(new SceneLayer() { Name = "new_layer_1" });
            UpdateList();
        }
    }
}
