using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;

namespace Compounder
{
    public class ArrowSceneObject : AbstractSceneObject, ISceneObject
    {
        public ArrowSceneObject() { }
        public ArrowSceneObject(XElement el)
        {
            ZOrder = el.Attribute("zOrder").Value.ToDouble();

            var loc = el.Element("source");
            Source.RelativePositon = new Vector2d(loc.Element("x").Value.ToDouble(),
                loc.Element("y").Value.ToDouble());
        }

        public ConnectorPoint Source { get; set; } = new ConnectorPoint();
        public ConnectorPoint Target { get; set; } = new ConnectorPoint();
        public Vector2d Location => Source.Location;
        PointF LocationF => new PointF(Location.X.ToFloat(), Location.Y.ToFloat());

        public bool CheckHovered(DrawingContext dc, CursorPosition curp)
        {
            var location = curp.World;
            var bbox = GetBBox();
            var rect = new RectangleF(Location.X.ToFloat(), Location.Y.ToFloat(), bbox.Width.ToFloat(), bbox.Height.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);
            var bbox = GetBBox();
            var rect = new RectangleF(t0.X, t0.Y, bbox.Width.ToFloat() * dc.zoom, bbox.Height.ToFloat() * dc.zoom);

            var trans = dc.gr.Transform;
            
            var from = dc.Transform(Source.Location);
            var to = dc.Transform(Target.Location);
            int cw = 12; float arrowWidth = 5.0f;
            float arrowHeight = 5.0f;
            bool isFilled = true; // Set to true for a filled arrow
            AdjustableArrowCap myArrow = new AdjustableArrowCap(arrowWidth, arrowHeight, isFilled);

            // You can further adjust properties after creation
            myArrow.Width = 10.0f;
            myArrow.Height = 15.0f;
            myArrow.MiddleInset = 2.0f;

            // 2. Create a Pen object
            Pen p = new Pen(Color.LightGreen, 3);

            // 3. Assign the custom cap to the start or end of the line
           
            if (IsSelected)
            {
                p = new Pen(Color.LightBlue, 3);
                //dc.gr.DrawRectangle(new Pen(Color.LightGreen, 3), rect);
            }
            else if (CheckHovered(dc, dc.GetCursor()))
            {
                p = new Pen(Color.Red, 3);
                // dc.gr.DrawRectangle(new Pen(Color.Red, 3), rect);
            }
            p.CustomEndCap = myArrow;

            dc.gr.DrawLine(p, from.X, from.Y, to.X, to.Y);
            dc.gr.DrawEllipse(Pens.Blue, from.X - cw / 2, from.Y - cw / 2, cw, cw);
            dc.gr.DrawEllipse(Pens.Blue, to.X - cw / 2, to.Y - cw / 2, cw, cw);

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
                    var dd = AutoDialog.DialogHelpers.StartDialog();
                    
                    dd.AddNumericField("z", "ZOrder", ZOrder, min: -1000);

                    if (!dd.ShowDialog())
                    {
                        ev.Handled = true;
                        return;
                    }

                    
                    ZOrder = dd.GetNumericField("z");

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
            ret.Add(new XAttribute("kind", "arrow"));


            
            ret.Add(new XAttribute("zOrder", ZOrder));

            XElement loc = new XElement("source");
            ret.Add(loc);
            loc.Add(new XElement("x", Location.X));
            loc.Add(new XElement("y", Location.Y));
            return ret;
        }


        public BBox GetBBox()
        {
            var bbox = Source.GetBBox().Combine(Target.GetBBox());
            return bbox;

        }
    }
}
