using OpenTK.GLControl;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Compounder
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Form = this;
            menu = new RibbonMenu();
            tableLayoutPanel1.Controls.Add(menu, 0, 0);
            menu.Height = 115;
            menu.Dock = DockStyle.Top;
            _timer = new System.Timers.Timer(10.0); // in milliseconds - you might have to play with this value to throttle your framerate depending on how involved your update and render code is
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            var mf = new MessageFilter();
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Paint += PictureBox1_Paint;
            System.Windows.Forms.Application.AddMessageFilter(mf);
            dc.Init(pictureBox1);
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
        }

        private void PictureBox1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            var curp = dc.GetCursor();
            var me = new UiMouseDoubleClickEvent() { Button = e.Button, Location = curp };
            if (e.Button == MouseButtons.Left)
            {
                foreach (var item in Objects)
                {
                    item.IsSelected = false;
                }
            }
            var ord = Objects.OrderBy(z => z.ZOrder).ToArray();
            foreach (var item in ord.Reverse())
            {
                item.Event(dc, me);
                if (me.Handled)
                    break;
            }
        }

        private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
        {
            var curp = dc.GetCursor();
            var me = new UiMouseEvent() { Button = e.Button, Location = curp };
            if (e.Button == MouseButtons.Left)
            {
                foreach (var item in Objects)
                {
                    item.IsSelected = false;
                }
            }
            var ord = Objects.OrderBy(z => z.ZOrder).ToArray();
            foreach (var item in ord.Reverse())
            {
                item.Event(dc, me);
                if (me.Handled)
                    break;
            }
        }

        DrawingContext dc = new DrawingContext();

        CompounderProject project = new CompounderProject();

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            dc.gr = e.Graphics;
            var gr = e.Graphics;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.Clear(Color.White);
            var t0 = dc.Transform(0, 0);
            var t1 = dc.Transform(100, 0);
            var t2 = dc.Transform(0, 100);
            dc.gr.DrawLine(Pens.Green, t0, t1);
            dc.gr.DrawLine(Pens.Red, t0, t2);
            foreach (var item in Objects.OrderBy(z => z.ZOrder))
            {
                item.Draw(dc);
            }


            dc.UpdateDrag();
        }

        System.Timers.Timer _timer;
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //UpdateModel(); // this is where you'd do whatever you need to do to update your model per frame
            // Invalidate will cause the Paint event on your GLControl to fire
            pictureBox1.Invalidate(); // _glControl is obviously a private reference to the GLControl
        }
        List<ISceneObject> Objects = new List<ISceneObject>();
        internal void ImportImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            Objects.Add(new ImageSceneObject() { Bitmap = (Bitmap)Bitmap.FromFile(ofd.FileName) });
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteSelected();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        internal void DeleteSelected()
        {
            Objects.RemoveAll(z => z.IsSelected);
        }

        internal void MoveSelected()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("x", "X");
            d.AddNumericField("y", "Y");
            if (!d.ShowDialog())
                return;

            var offset = new Vector2d(d.GetNumericField("x"), d.GetNumericField("y"));
            foreach (var item in Objects.Where(z => z.IsSelected))
            {
                item.Location += offset;
            }
        }

        internal void CreateRect()
        {
            Objects.Add(new RectObject() { Width = 100, Height = 50, Text = "rect01" });
        }

        public static Form2 Form;
        RibbonMenu menu;

        private void sendBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var minZorder = Objects.Min(z => z.ZOrder) + 1;
            foreach (var item in Objects.Where(z => z.IsSelected))
            {
                item.ZOrder -= minZorder;
            }
        }
    }
}
