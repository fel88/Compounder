using Compounder.Interfaces;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace Compounder
{
    public class RectObject : AbstractSceneObject, ISceneObject, IOffsetableSceneObject
    {
        public RectObject() { }
        public RectObject(XElement elem)
        {

            ZOrder = elem.Attribute("zOrder").Value.ToDouble();
            Width = elem.Attribute("width").Value.ToDouble();
            Height = elem.Attribute("height").Value.ToDouble();
            Rotate = elem.Attribute("rotate").Value.ToDouble();
            Fill = bool.Parse(elem.Attribute("fill").Value);
            if (elem.Element("text") != null)
                Text = elem.Element("text").Value;

            var loc = elem.Element("location");
            Location = new Vector2d(loc.Element("x").Value.ToDouble(),
                loc.Element("y").Value.ToDouble());

        }
        public Vector2d Location { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotate { get; set; }
        public string Text { get; set; }
        public bool Fill { get; set; }
        public bool CheckHovered(DrawingContext dc, CursorPosition curp)
        {
            var location = curp.World;
            var rect = new RectangleF(Location.X.ToFloat(), Location.Y.ToFloat() - Height.ToFloat(), Width.ToFloat(), Height.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);
            var rect = new RectangleF(t0.X, t0.Y, Width.ToFloat() * dc.zoom, Height.ToFloat() * dc.zoom);

            var trans = dc.gr.Transform;
            dc.gr.TranslateTransform(t0.X, t0.Y);

            dc.gr.RotateTransform(Rotate.ToFloat());
            dc.gr.TranslateTransform(-t0.X, -t0.Y);

            dc.gr.DrawRectangle(new Pen(Color.Black, 3), rect);
            if (Fill)
                dc.gr.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.Yellow)), rect);
            dc.gr.Transform = trans;

            dc.gr.DrawString(Text, new Font("Consolas", 18), Brushes.Black, rect.Location);

            if (IsSelected)
            {
                dc.gr.DrawRectangle(new Pen(Color.LightGreen, 3), rect);
            }
            else if (CheckHovered(dc, dc.GetCursor()))
            {
                dc.gr.DrawRectangle(new Pen(Color.Red, 3), rect);
            }

        }

        public void Event(IUIEvent ev)
        {
            var dc = ev.DrawingContext;
            if (ev is UiMouseDoubleClickEvent mdc)
            {
                /*var d = AutoDialog.DialogHelpers.StartDialog();
                d.AddOptionsField("command", "Command", ["resize"], 0);
                if (!d.ShowDialog())
                    return;

                var idx = d.GetOptionsFieldIdx("command");
                if (idx == 0)*/
                {
                    if (CheckHovered(dc, mdc.Location))
                    {
                        ev.Handled = true;

                        var dd = AutoDialog.DialogHelpers.StartDialog();
                        dd.AddNumericField("width", "Width", Width);
                        dd.AddNumericField("height", "Height", Height);
                        dd.AddNumericField("rotate", "Rotate", Rotate);
                        dd.AddNumericField("z", "ZOrder", ZOrder);
                        dd.AddStringField("text", "Text", Text);
                        dd.AddBoolField("fill", "Fill", Fill);

                        if (!dd.ShowDialog())
                            return;

                        ZOrder = dd.GetNumericField("z");
                        Width = dd.GetNumericField("width");
                        Height = dd.GetNumericField("height");
                        Rotate = dd.GetNumericField("rotate");
                        Text = dd.GetStringField("text");
                        Fill = dd.GetBoolField("fill");
                    }
                }
            }
            else if (ev is UiMouseEvent mev)
            {
                if (mev.Button == MouseButtons.Left)
                {
                    IsSelected = CheckHovered(dc, mev.Location);
                    if (IsSelected)
                        ev.Handled = true;
                    return;
                }
            }
        }

        public BBox GetBBox()
        {
            return new BBox(Location.X, Location.Y - Height, Width, Height);
        }

        public void Offset(Vector2d v)
        {
            Location += v;
        }

        public void SetLocation(Vector2d location)
        {
            Location = location;
        }

        public XElement ToXml()
        {
            XElement ret = new XElement("item");
            ret.Add(new XAttribute("kind", "rect"));

            ret.Add(new XAttribute("zOrder", ZOrder));
            ret.Add(new XAttribute("fill", Fill));
            ret.Add(new XAttribute("width", Width));
            ret.Add(new XAttribute("height", Height));
            ret.Add(new XAttribute("rotate", Rotate));
            ret.Add(new XElement("text", new XCData(Text)));


            XElement loc = new XElement("location");
            ret.Add(loc);
            loc.Add(new XElement("x", Location.X));
            loc.Add(new XElement("y", Location.Y));
            return ret;
        }
    }
}
