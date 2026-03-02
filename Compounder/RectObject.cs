using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Compounder
{
    public class RectObject : AbstractSceneObject, ISceneObject, IOffsetableSceneObject
    {
        public Vector2d Location{ get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; }
        public bool CheckHovered(DrawingContext dc, Vector2d location)
        {
            var rect = new RectangleF(Location.X.ToFloat(), Location.Y.ToFloat() - Height.ToFloat(), Width.ToFloat(), Height.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);
            var rect = new RectangleF(t0.X, t0.Y, Width.ToFloat() * dc.zoom, Height.ToFloat() * dc.zoom);

            dc.gr.DrawRectangle(new Pen(Color.Black, 3), rect);
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
                        dd.AddStringField("text", "Text", Text);
                        if (!dd.ShowDialog())
                            return;
                        Width = dd.GetNumericField("width");
                        Height = dd.GetNumericField("height");
                        Text = dd.GetStringField("text");
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

        public XElement ToXml()
        {
            XElement ret = new XElement("rect");
            return ret;
        }
    }
}
