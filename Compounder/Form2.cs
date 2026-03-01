using OpenTK.GLControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Compounder
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
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
        }

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            var gr = e.Graphics;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.Clear(Color.White);
            for (int i = 0; i < 10; i++)
            {
                gr.DrawRectangle(Pens.Black, i * 10, i * 10, i * 20, i * 20);
            }
        }

        System.Timers.Timer _timer;
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //UpdateModel(); // this is where you'd do whatever you need to do to update your model per frame
            // Invalidate will cause the Paint event on your GLControl to fire
            pictureBox1.Invalidate(); // _glControl is obviously a private reference to the GLControl
        }
        public static Form2 Form;
        RibbonMenu menu;
    }
}
