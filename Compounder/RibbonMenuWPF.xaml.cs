using Dagre;
using OpenTK.Mathematics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compounder
{
    /// <summary>
    /// Interaction logic for RibbonMenuWPF.xaml
    /// </summary>
    public partial class RibbonMenuWPF : System.Windows.Controls.UserControl
    {
        public RibbonMenuWPF()
        {
            InitializeComponent();
            RibbonWin.Loaded += RibbonMenuWPF_Loaded;
        }

        private void RibbonMenuWPF_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }
     
        public Form2 Form => Form2.Form;

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ab.ShowDialog();
        }

        public List<AItem> AllItems = new List<AItem>();

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllItems.Count == 0)
                return;
            DagreInputGraph dg = new DagreInputGraph();
            foreach (var item in AllItems)
            {
                dg.AddNode(item, item.Rect.Width, item.Rect.Height);
            }
            foreach (var item in AllItems)
            {
                foreach (var ch in item.Childs)
                {
                    var nd1 = dg.GetNode(item);
                    var nd2 = dg.GetNode(ch);
                    dg.AddEdge(nd1, nd2);
                }
            }


            dg.Layout();

            foreach (var item in dg.Nodes())
            {
                var tag = (item.Tag as AItem);
                tag.X = item.X;
                tag.Y = -item.Y;
            }

            fitAll();
        }

        private void fitAll()
        {

        }

        private void ImportImage_click(object sender, RoutedEventArgs e)
        {
            Form.ImportImage();
        }

        private void SaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            Form.SaveAsProject();
        }

        private void OpenProject_click(object sender, RoutedEventArgs e)
        {
            Form.OpenProject();
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            Form.DeleteSelected();
        }

        private void Move_click(object sender, RoutedEventArgs e)
        {
            Form.MoveSelected();
        }

        private void Rect_click(object sender, RoutedEventArgs e)
        {
            Form.CreateRect();
        }

        private void Undo_click(object sender, RoutedEventArgs e)
        {
            Form.Undo();
        }

        private void Arrow_click(object sender, RoutedEventArgs e)
        {
            Form.CreateArrow();
        }
    }

}