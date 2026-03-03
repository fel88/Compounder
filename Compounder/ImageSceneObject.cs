using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;

namespace Compounder
{
    public class ImageSceneObject : AbstractSceneObject, ISceneObject,IOffsetableSceneObject
    {
        public ImageSceneObject() { }
        public ImageSceneObject(XElement el)
        {
            ImgScale = el.Attribute("scale").Value.ToDouble();
            ZOrder = el.Attribute("zOrder").Value.ToDouble();
            var data = Convert.FromBase64String(el.Element("bmp").Value);
            MemoryStream ms = new MemoryStream(data);
            Bitmap = new Bitmap(ms);
            var loc = el.Element("location");
            Location = new Vector2d(loc.Element("x").Value.ToDouble(),
                loc.Element("y").Value.ToDouble());
        }
        public Bitmap Bitmap;
        public Vector2d Location { get; set; }
        PointF LocationF => new PointF(Location.X.ToFloat(), Location.Y.ToFloat());

        public bool CheckHovered(DrawingContext dc, CursorPosition curp)
        {
            var location = curp.World;
            var rect = new RectangleF(Location.X.ToFloat(), Location.Y.ToFloat() - Bitmap.Height * ImgScale.ToFloat(), Bitmap.Width * ImgScale.ToFloat(), Bitmap.Height * ImgScale.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);
            var rect = new RectangleF(t0.X, t0.Y, Bitmap.Width * dc.zoom * ImgScale.ToFloat(), Bitmap.Height * dc.zoom * ImgScale.ToFloat());

            var trans = dc.gr.Transform;
            dc.gr.TranslateTransform(t0.X, t0.Y);

            dc.gr.RotateTransform(Rotate.ToFloat());
            dc.gr.TranslateTransform(-t0.X, -t0.Y);

            dc.gr.DrawImage(Bitmap, rect);
            dc.gr.Transform = trans;
            if (IsSelected)
            {
                dc.gr.DrawRectangle(new Pen(Color.LightGreen, 3), rect);
            }
            else if (CheckHovered(dc, dc.GetCursor()))
            {
                dc.gr.DrawRectangle(new Pen(Color.Red, 3), rect);
            }

        }
        double ImgScale = 1;
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
                    var dd = AutoDialog.DialogHelpers.StartDialog();
                    dd.AddNumericField("scale", "Scale", ImgScale);
                    dd.AddNumericField("z", "ZOrder", ZOrder, min: -1000);
                    dd.AddNumericField("rotate", "Rotate", Rotate, min: -1000);
                    if (!dd.ShowDialog())
                    {
                        ev.Handled = true;
                        return;
                    }

                    ImgScale = dd.GetNumericField("scale");
                    ZOrder = dd.GetNumericField("z");
                    Rotate = dd.GetNumericField("rotate");
                    ev.Handled = true;
                    return;
                }
            }
            else if (ev is UiMouseEvent mev)
            {
                if (mev.Button == MouseButtons.Left)
                {
                    if ((Control.ModifierKeys & Keys.Control) != 0)// xor mode
                    {
                        if (CheckHovered(dc, mev.Location))
                        {
                            IsSelected = !IsSelected;
                            ev.Handled = true;
                        }
                    }
                    else
                    {
                        var res = CheckHovered(dc, mev.Location);
                        IsSelected = res;

                        if (IsSelected && res)
                            ev.Handled = true;
                    }

                    return;
                }
            }
        }

        public XElement ToXml()
        {
            XElement ret = new XElement("item");
            ret.Add(new XAttribute("kind", "image"));
            var data = new XElement("bmp");
            MemoryStream ms = new MemoryStream();
            Bitmap.Save(ms, ImageFormat.Png);
            ret.Add(new XAttribute("scale", ImgScale));
            ret.Add(new XAttribute("zOrder", ZOrder));
            ms.Seek(0, SeekOrigin.Begin);
            string base64String = Convert.ToBase64String(ms.ToArray());
            data.Add(new XCData(base64String));
            ret.Add(data);
            XElement loc = new XElement("location");
            ret.Add(loc);
            loc.Add(new XElement("x", Location.X));
            loc.Add(new XElement("y", Location.Y));
            return ret;
        }

        public double Rotate { get; set; }
        public BBox GetBBox()
        {
            return new BBox(Location.X, Location.Y - Bitmap.Height, Bitmap.Width, Bitmap.Height);

        }

        public void Offset(Vector2d v)
        {
            Location += v;
        }

        public void SetLocation(Vector2d location)
        {
            Location = location;
        }
    }
}
