using Compounder.Interfaces;
using OpenTK.GLControl;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Compounder
{
    public partial class Form2 : Form, IEditor
    {
        public Form2()
        {
            InitializeComponent();
            Form = this;
            dc = new DrawingContext() { Editor = this };
            var ll = new LayersList();
            ll.Init(this);
            dc.ZoomFactor = 1.2f;
            tableLayoutPanel1.Controls.Add(ll, 1, 1);
            ll.Dock = DockStyle.Fill;
            menu = new RibbonMenu();

            tableLayoutPanel1.Controls.Add(menu, 0, 0);
            tableLayoutPanel1.SetColumnSpan(menu, 2);
            menu.Height = 115;
            menu.Dock = DockStyle.Top;
            _timer = new System.Timers.Timer(10.0); // in milliseconds - you might have to play with this value to throttle your framerate depending on how involved your update and render code is
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            var mf = new MessageFilter();
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            System.Windows.Forms.Application.AddMessageFilter(mf);
            dc.Init(pictureBox1);
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
        }

        private void PictureBox1_MouseWheel(object? sender, MouseEventArgs e)
        {
            dirty = true;
        }

        private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
        {
            dirty = true;
        }

        private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
        {
            dirty = true;
            if (e.Button == MouseButtons.Right)
            {
                if ((Math.Abs(dc.lastDragDiffX) + Math.Abs(dc.lastDragDiffY)) < 3)
                    contextMenuStrip1.Show(pictureBox1, e.Location);
            }

            var curp = dc.GetCursor();
            var me = new UiMouseClickEvent(dc, this, this) { Button = e.Button, Location = curp, Type = UiMouseClickEvent.UiMouseEventTypeEnum.ButtonUp };
            if (_currentTool != null)
            {
                _currentTool.MouseUp(me);
                return;
            }
            var ord = Objects.OrderBy(z => z.ZOrder).ToArray();
            foreach (var item in ord.Reverse())
            {
                item.Event(me);
                if (me.Handled)
                    break;
            }
        }

        private void PictureBox1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            dirty = true;
            if ((Control.ModifierKeys & Keys.Control) != 0)
                return;
            var curp = dc.GetCursor();
            var me = new UiMouseDoubleClickEvent(dc, this, this) { Button = e.Button, Location = curp };
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
                if (item.CheckHovered(dc, curp))
                    item.Event(me);
                if (me.Handled)
                    break;
            }
        }

        private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
        {
            dirty = true;
            var curp = dc.GetCursor();
            var me = new UiMouseClickEvent(dc, this, this) { Button = e.Button, Location = curp, Type = UiMouseClickEvent.UiMouseEventTypeEnum.ButtonDown };
            if (_currentTool != null)
            {
                _currentTool.MouseDown(me);
                return;
            }
            bool skip = false;
            if (moveAnchor != null && moveAnchor.CheckHovered(dc, curp))
                skip = true;

            if (!skip && e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == 0)
            {
                foreach (var item in Objects)
                {
                    item.IsSelected = false;
                }
            }
            var ord = Objects.OrderBy(z => z.ZOrder).ToArray();
            foreach (var item in ord.Reverse())
            {
                item.Event(me);
                if (me.Handled)
                    break;
            }
        }

        DrawingContext dc = null;

        public CompounderProject Project => project;
        CompounderProject project = new CompounderProject();

        MoveAnchor moveAnchor = null;

        public ISceneObject[] GetSelected()
        {
            return (Objects.Where(z => z.IsSelected)).ToArray();
        }

        private void PictureBox1_Paint(object? sender, PaintEventArgs e)
        {
            dc.gr = e.Graphics;
            var gr = e.Graphics;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.Clear(Color.White);
            var t0 = dc.Transform(0, 0);
            var t1 = dc.Transform(100, 0);
            var t2 = dc.Transform(0, 100);
            dc.gr.DrawLine(Pens.Green, t0.ToPointF(), t1.ToPointF());
            dc.gr.DrawLine(Pens.Red, t0.ToPointF(), t2.ToPointF());
            var curp = dc.GetCursor();
            toolStripStatusLabel1.Text = curp.World.X + "; " + curp.World.Y;
            var allSelected = (Objects.Where(z => z.IsSelected)).OfType<IOffsetableSceneObject>().ToArray();
            var bboxes = allSelected.Select(z => z.GetBBox()).ToArray();
            if (bboxes.Any())
            {
                var combinedBbox = bboxes.Aggregate((z, y) => z.Combine(y));
                if (combinedBbox.Area > 0)
                {
                    var pen = new Pen(Color.LightGreen, 2);
                    pen.DashPattern = [5, 5, 10, 10];
                    if (allSelected.Count() > 1)//draw outskirt
                    {
                        var t00 = dc.Transform(combinedBbox.Location);
                        var rect = new RectangleF(t0.X.ToFloat(), t0.Y.ToFloat(), combinedBbox.Width.ToFloat() * dc.zoom, combinedBbox.Height.ToFloat() * dc.zoom);
                        //dc.gr.DrawRectangle(pen, rect);
                    }


                    if (moveAnchor == null)
                        moveAnchor = new MoveAnchor() { Location = combinedBbox.Location };
                    else
                        moveAnchor.Location = combinedBbox.Location;
                    if (!VirtualObjects.Contains(moveAnchor))
                    {
                        //draw snaps: rotate, move
                        VirtualObjects.Add(moveAnchor);
                    }

                }
            }
            else
            {
                if (moveAnchor != null)
                {
                    VirtualObjects.Remove(moveAnchor);
                    moveAnchor = null;
                }
            }

            var mme = new UiMouseMoveEvent(dc, this, this) { Location = dc.GetCursor() };
            var ord = Objects.OrderBy(z => z.ZOrder).ToArray();
            foreach (var item in Objects)
            {
                item.IsHovered = false;
            }
            foreach (var item in ord.Reverse())
            {
                if (!item.CheckHovered(dc, mme.Location))
                    continue;

                item.Event(mme);
                if (mme.Handled)
                    break;
            }
            //groups routine

            var groups = Objects.Where(z => z.IsHovered && z.Group != null).Select(z => z.Group).Distinct().ToArray();
            foreach (var item in groups)
            {
                var objs = Objects.Where(z => z.Group == item).ToArray();
                foreach (var item2 in objs)
                {
                    item2.IsHovered = true;
                }
            }
            foreach (var item in Objects.OrderBy(z => z.ZOrder))
            {
                item.Draw(dc);
            }
            _currentTool?.Draw(dc);

            //post draw effects
            if (ShowLinesBetweenGroupsElementsWhenHover || ShowLinesBetweenAllGroupsElements)
                DrawGroupLines();

            DrawTooltip();

            dc.UpdateDrag();
            dirty = false;
        }
        ArrowSceneObject.CurveTypeEnum ArrowBetweenElementsCurveType = ArrowSceneObject.CurveTypeEnum.Rect;
        private void DrawGroupLines()
        {
            var groups = Objects.Where(z => z.IsHovered && z.Group != null).Select(z => z.Group).Distinct().ToArray();
            if (ShowLinesBetweenAllGroupsElements)
                groups = Objects.Where(z => z.Group != null).Select(z => z.Group).Distinct().ToArray();

            foreach (var item in groups)
            {
                var objs = Objects.Where(z => z.Group == item).ToArray();
                for (int i = 0; i < objs.Length; i++)
                {
                    var objA = objs[i];
                    for (int j = i + 1; j < objs.Length; j++)
                    {
                        var objB = objs[j];
                        //draw line 
                        var temp = new ArrowSceneObject()
                        {
                            IsHoverable = false,
                            DrawEndCap = false,
                            CurveType = ArrowBetweenElementsCurveType
                        };
                        temp.Source.RelativePositon = objA.GetBBox().Center;
                        temp.Target.RelativePositon = objB.GetBBox().Center;
                        temp.Draw(dc);
                    }
                }
            }
        }

        private void DrawTooltip()
        {
            foreach (var item in Objects.OfType<RectObject>())
            {
                if (!item.IsHovered)
                    continue;

                if (!item.CheckHovered(dc, dc.GetCursor()))
                    continue;

                if (string.IsNullOrEmpty(item.Text))
                    continue;

                var font = new System.Drawing.Font("Consolas", 18);
                var fontBold = new System.Drawing.Font("Consolas", 18, System.Drawing.FontStyle.Bold);
                var pos1 = dc.GetCursor().Screen.ToPointF();
                var totalText = item.Text;
                if (!string.IsNullOrEmpty(item.Description))
                    totalText += "\\n" + item.Description;

                var lines = totalText.Split("\\n").ToArray();
                double yy = 0;
                List<SizeF> sizes = new List<SizeF>();
                foreach (var mitem in lines)
                {
                    sizes.Add(dc.gr.MeasureString(mitem, font));
                }
                var maxx = sizes.Max(z => z.Width);
                dc.gr.FillRectangle(Brushes.AliceBlue, pos1.X, pos1.Y - sizes.Sum(z => z.Height), maxx, sizes.Sum(z => z.Height));
                for (int i = 0; i < lines.Length; i++)
                {
                    var mss = sizes[i];
                    dc.gr.DrawString(lines[i], (i == 0 && lines.Length > 1) ? fontBold : font, Brushes.Black, pos1.X, (int)yy + pos1.Y - sizes.Sum(z => z.Height));
                    yy += mss.Height;
                }
            }
        }

        System.Timers.Timer _timer;
        bool dirty = false;
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //UpdateModel(); // this is where you'd do whatever you need to do to update your model per frame
            // Invalidate will cause the Paint event on your GLControl to fire
            if (dirty)
                pictureBox1.Invalidate(); // _glControl is obviously a private reference to the GLControl
        }
        public List<ISceneObject> VirtualObjects { get; private set; } = new List<ISceneObject>();
        IReadOnlyList<ISceneObject> Objects => project.Objects.Concat(VirtualObjects).ToList();

        public ITool CurrentTool => _currentTool;

        public SceneLayer ActiveLayer { get; set; }

        internal void ImportImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            project.Objects.Add(new ImageSceneObject() { Bitmap = (Bitmap)Bitmap.FromFile(ofd.FileName) });
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteSelected();
            }
            dirty = true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        internal void DeleteSelected()
        {
            StackState();
            var toDel = Objects.Where(z => z.IsSelected).ToArray();
            foreach (var item in toDel)
            {
                VirtualObjects.Remove(item);
                if (item.Childs != null)
                    foreach (var citem in item.Childs)
                    {
                        VirtualObjects.Remove(citem);
                    }
            }
            project.Objects.RemoveAll(z => z.IsSelected);
        }

        internal void MoveSelected()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("x", "X");
            d.AddNumericField("y", "Y");
            if (!d.ShowDialog())
                return;

            var offset = new Vector2d(d.GetNumericField("x"), d.GetNumericField("y"));
            foreach (var item in Objects.OfType<IOffsetableSceneObject>().Where(z => z.IsSelected))
            {
                item.Offset(offset);
            }
        }

        internal void CreateRect()
        {
            SetTool(new RectCreationTool(this, dc));
            //            var pn = dc.BackTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            //          project.Objects.Add(new RectObject() { Width = 100, Height = 50, Text = "rect01", Location = pn });
        }

        public static Form2 Form;
        RibbonMenu menu;

        private void sendBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var minZorder = project.Objects.Min(z => z.ZOrder) - 1;
            foreach (var item in project.Objects.Where(z => z.IsSelected))
            {
                item.ZOrder = minZorder;
            }
        }

        public void OpenProject()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            project = new CompounderProject(XDocument.Load(ofd.FileName).Root);
        }

        internal void SaveAsProject()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var xml = project.ToXml();
            File.WriteAllText(sfd.FileName, xml.ToString());
        }

        public void StackState()
        {
            undoStack.Push(project.ToXml());
        }

        Stack<XElement> undoStack = new Stack<XElement>();
        internal void Undo()
        {
            if (undoStack.Count == 0)
                return;

            project = new CompounderProject(undoStack.Pop());
        }

        internal void CreateArrow()
        {
            SetTool(new ArrowCreationTool(this, dc));
            //project.Objects.Add(new ArrowSceneObject() { Source = new ConnectorPoint() { RelativePositon = new Vector2d(0, 0) }, Target = new ConnectorPoint() { RelativePositon = new Vector2d(100, 100) } });
        }

        ITool DefaultTool = null;
        public void ResetTool()
        {
            _currentTool?.Deselect();
            SetTool(DefaultTool);
        }

        ITool _currentTool = null;
        public void SetTool(ITool tool)
        {
            _currentTool?.Deselect();
            _currentTool = tool;
            _currentTool?.Select();
        }

        private void bringFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var maxZorder = project.Objects.Max(z => z.ZOrder) + 1;
            foreach (var item in project.Objects.Where(z => z.IsSelected))
            {
                item.ZOrder = maxZorder;
            }
        }

        internal void ClearProject()
        {
            project.Objects.Clear();
            VirtualObjects.Clear();
        }

        internal void GroupSelected()
        {
            var gr = new SceneGroup();
            foreach (var item in Objects.Where(z => z.IsSelected))
            {
                item.Group = gr;
            }
        }

        internal void UnGroupSelected()
        {
            foreach (var item in Objects.Where(z => z.IsSelected))
            {
                item.Group = null;
            }
        }

        internal void SwitchLayersPanelVisible()
        {
            if (tableLayoutPanel1.ColumnStyles[1].Width == 0)
            {
                tableLayoutPanel1.ColumnStyles[1].Width = 300;
            }
            else
                tableLayoutPanel1.ColumnStyles[1].Width = 0;
        }

        bool ShowLinesBetweenGroupsElementsWhenHover = false;
        bool ShowLinesBetweenAllGroupsElements = false;
        internal void GroupSettings()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddBoolField("ShowLinesBetweenGroupsElementsWhenHover", "ShowLinesBetweenGroupsElementsWhenHover", ShowLinesBetweenGroupsElementsWhenHover);
            d.AddBoolField("ShowLinesBetweenAllGroupsElements", "ShowLinesBetweenAllGroupsElements", ShowLinesBetweenAllGroupsElements);
            d.AddOptionsField("ArrowBetweenElementsCurveType", "ArrowBetweenElementsCurveType", Enum.GetNames<ArrowSceneObject.CurveTypeEnum>(), ArrowBetweenElementsCurveType.ToString());

            if (!d.ShowDialog())
                return;

            ShowLinesBetweenGroupsElementsWhenHover = d.GetBoolField("ShowLinesBetweenGroupsElementsWhenHover");
            ShowLinesBetweenAllGroupsElements = d.GetBoolField("ShowLinesBetweenAllGroupsElements");
            ArrowBetweenElementsCurveType = Enum.Parse<ArrowSceneObject.CurveTypeEnum>(d.GetOptionsField("ArrowBetweenElementsCurveType"));
        }

        internal void FitAll()
        {
            if (!Objects.Any())
                return;
            var combinedBbox = Objects.Select(z => z.GetBBox()).Aggregate((z, y) => z.Combine(y));
            const int gap = 10;
            dc.FitToPoints(new PointF[]
            {
                new PointF (combinedBbox.Location.X.ToFloat()-gap,combinedBbox.Location.Y.ToFloat()-gap),
                new PointF (combinedBbox.Right.ToFloat()+gap,combinedBbox.Bottom.ToFloat()+gap)
            });
            dirty = true;
        }
    }
}
