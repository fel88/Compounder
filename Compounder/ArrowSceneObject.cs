using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;

namespace Compounder
{
    public class ArrowSceneObject : AbstractSceneObject, ISceneObject
    {
        public ArrowSceneObject()
        {
            InitAnchors();
        }
        public ArrowSceneObject(XElement el)
        {
            ZOrder = el.Attribute("zOrder").Value.ToDouble();

            var loc = el.Element("source");
            Source.RelativePositon = new Vector2d(loc.Element("x").Value.ToDouble(), loc.Element("y").Value.ToDouble());

            if (el.Attribute("drawEndCap") != null)
                DrawEndCap = bool.Parse(el.Attribute("drawEndCap").Value);

            if (el.Attribute("lineWidth") != null)
                LineWidth = el.Attribute("lineWidth").Value.ToDouble().ToFloat();

            if (el.Element("target") != null)
            {
                var loc2 = el.Element("target");
                Target.RelativePositon = new Vector2d(loc2.Element("x").Value.ToDouble(), loc2.Element("y").Value.ToDouble());
            }

            if (el.Attribute("curveType") != null)                            
                CurveType = Enum.Parse<CurveTypeEnum>(el.Attribute("curveType").Value);
            

            InitAnchors();

            TargetAnchor.Location = Target.RelativePositon;
            SourceAnchor.Location = Source.RelativePositon;
        }
        public bool DrawEndCap { get; set; } = true;
        private void InitAnchors()
        {
            TargetAnchor = new ArrowMoveAnchor(this, (x) =>
            {
                if (Source.Parent == null)
                {
                    Target.RelativePositon = TargetAnchor.Location;
                }
                //Source.Location = TargetAnchor.Location;
            });
            SourceAnchor = new ArrowMoveAnchor(this, (x) =>
            {
                if (Source.Parent == null)
                {
                    Source.RelativePositon = SourceAnchor.Location;
                }
            });
            Childs = [TargetAnchor, SourceAnchor];
        }
        public ArrowMoveAnchor TargetAnchor { get; set; }
        public ArrowMoveAnchor SourceAnchor { get; set; }

        public ConnectorPoint Source { get; set; } = new ConnectorPoint();
        public ConnectorPoint Target { get; set; } = new ConnectorPoint();
        public Vector2d Location => Source.Location;
        PointF LocationF => new PointF(Location.X.ToFloat(), Location.Y.ToFloat());

        public bool CheckHovered(DrawingContext dc, CursorPosition curp)
        {
            if (!IsHoverable)
                return false;
            var location = curp.World;
            var bbox = GetBBox();
            var rect = new RectangleF(bbox.Location.X.ToFloat(), bbox.Location.Y.ToFloat(), bbox.Width.ToFloat(), bbox.Height.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public float LineWidth = 3;
        public bool IsHoverable = true;

        public void Draw(DrawingContext dc)
        {

            var bbox = GetBBox();
            var t0 = dc.Transform(bbox.Location);
            var rect = new RectangleF(t0.X, t0.Y - bbox.Height.ToFloat() * dc.zoom, bbox.Width.ToFloat() * dc.zoom, bbox.Height.ToFloat() * dc.zoom);

            var trans = dc.gr.Transform;

            var from = dc.Transform(Source.Location);
            var to = dc.Transform(Target.Location);
            int cw = 12;
            float arrowWidth = 5.0f;
            float arrowHeight = 5.0f;
            bool isFilled = true; // Set to true for a filled arrow
            AdjustableArrowCap myArrow = new AdjustableArrowCap(arrowWidth, arrowHeight, isFilled);

            // You can further adjust properties after creation
            myArrow.Width = 6.0f;
            myArrow.Height = 9.0f;
            myArrow.MiddleInset = 2.0f;
            bool hovered = CheckHovered(dc, dc.GetCursor());
            // 2. Create a Pen object
            Pen p = new Pen(Color.LightGreen, LineWidth);


            //dc.gr.DrawRectangle(new Pen(Color.LightGreen, 1), rect);
            if (IsSelected)
            {
                p = new Pen(Color.LightBlue, LineWidth);

            }
            else if (hovered)
            {
                p = new Pen(Color.Red, LineWidth);
                // dc.gr.DrawRectangle(new Pen(Color.Red, 3), rect);
            }

            if (DrawEndCap)
                p.CustomEndCap = myArrow;

            if (CurveType == CurveTypeEnum.Rect)
            {
                var midX = (to.X + from.X) / 2;

                dc.gr.DrawLines(p, [
                    new PointF(from.X,from.Y),
                    new PointF(midX,from.Y),
                    new PointF(midX,to.Y),
                    new PointF(to.X,to.Y),
                ]);


            }
            else
           if (CurveType == CurveTypeEnum.Line)
            {
                dc.gr.DrawLine(p, from.X, from.Y, to.X, to.Y);
            }
            else if (CurveType == CurveTypeEnum.Bezier)
            {
                var pos1 = from.ToVector2d();
                var rpos2 = to.ToVector2d();
                var vec = new Vector2d(pos1.X - rpos2.X, pos1.Y - rpos2.Y);
                var dir = vec.Normalized();
                Vector2d norm = new Vector2d(-dir.Y, dir.X);
                var cp1 = vec * (0.25f);
                var cp2 = vec * (0.75f);
                int side = (int)(vec.Length / 3f);
                var cx = (pos1.X + rpos2.X) / 2;

                cp1 = new Vector2d(rpos2.X + cp1.X + norm.X * side, rpos2.Y + cp1.Y + norm.Y * side);
                cp2 = new Vector2d(rpos2.X + cp2.X - norm.X * side, rpos2.Y + cp2.Y - norm.Y * side);

                cp1 = new Vector2d(cx, pos1.Y);
                cp2 = new Vector2d(cx, rpos2.Y);


                dc.gr.DrawBezier(p, pos1.ToPointF(), cp1.ToPointF(), cp2.ToPointF(), rpos2.ToPointF());
            }


            foreach (var item in Childs)
            {
                if (IsHovered || IsSelected || item.CheckHovered(dc, dc.GetCursor()))
                    item.Draw(dc);
            }

            //dc.gr.DrawEllipse(Pens.Blue, from.X - cw / 2, from.Y - cw / 2, cw, cw);
            // dc.gr.DrawEllipse(Pens.Blue, to.X - cw / 2, to.Y - cw / 2, cw, cw);

        }

        public enum CurveTypeEnum
        {
            Line, Bezier, Rect
        }
        public CurveTypeEnum CurveType { get; set; }
        public void Event(IUIEvent ev)
        {
            foreach (var item in Childs)
            {
                item.Event(ev);
            }
            var dc = ev.DrawingContext;
            if (ev is UiMouseMoveEvent mme)
            {
                IsHovered = CheckHovered(dc, mme.Location);
                if (IsHovered)
                    mme.Handled = true;
            }
            else
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
                    dd.AddNumericField("lineWidth", "Line width", LineWidth, min: 1);
                    dd.AddBoolField("drawEndCap", "Draw end cap", DrawEndCap);
                    dd.AddOptionsField("curveType", "Curve type", Enum.GetNames(typeof(CurveTypeEnum)), CurveType.ToString());

                    if (!dd.ShowDialog())
                    {
                        ev.Handled = true;
                        return;
                    }

                    DrawEndCap = dd.GetBoolField("drawEndCap");
                    ZOrder = dd.GetNumericField("z");
                    LineWidth = dd.GetNumericField("lineWidth").ToFloat();
                    CurveType = Enum.Parse<CurveTypeEnum>(dd.GetOptionsField("curveType"));
                    ev.Handled = true;
                    return;
                }
            }
            else if (ev is UiMouseClickEvent mev)
            {
                if (mev.Type == UiMouseClickEvent.UiMouseEventTypeEnum.ButtonUp && mev.Button == MouseButtons.Left)
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

        public XElement ToXml(ProjectXmlStoreContext ctx)
        {
            XElement ret = new XElement("item");
            ret.Add(new XAttribute("kind", "arrow"));



            ret.Add(new XAttribute("zOrder", ZOrder));
            ret.Add(new XAttribute("lineWidth", LineWidth));
            ret.Add(new XAttribute("curveType", CurveType));
            ret.Add(new XAttribute("drawEndCap", DrawEndCap));

            XElement loc = new XElement("source");

            ret.Add(loc);
            loc.Add(new XElement("x", Location.X));
            loc.Add(new XElement("y", Location.Y));
            XElement loc2 = new XElement("target");
            ret.Add(loc2);
            loc2.Add(new XElement("x", Target.Location.X));
            loc2.Add(new XElement("y", Target.Location.Y));
            return ret;
        }


        public BBox GetBBox()
        {
            var bbox = Source.GetBBox().Combine(Target.GetBBox());
            return bbox;

        }
    }
}
