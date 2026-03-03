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

            if (el.Element("target") != null)
            {
                var loc2 = el.Element("target");
                Target.RelativePositon = new Vector2d(loc2.Element("x").Value.ToDouble(), loc2.Element("y").Value.ToDouble());
            }
            if (el.Element("curveType") != null)
            {
                var type = el.Element("curveType");
                CurveType = Enum.Parse<CurveTypeEnum>(el.Element("curveType").Value);
            }

            InitAnchors();

            TargetAnchor.Location = Target.RelativePositon;
            SourceAnchor.Location = Source.RelativePositon;
        }

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
            var location = curp.World;
            var bbox = GetBBox();
            var rect = new RectangleF(bbox.Location.X.ToFloat(), bbox.Location.Y.ToFloat(), bbox.Width.ToFloat(), bbox.Height.ToFloat());
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {

            var bbox = GetBBox();
            var t0 = dc.Transform(bbox.Location);
            var rect = new RectangleF(t0.X, t0.Y - bbox.Height.ToFloat() * dc.zoom, bbox.Width.ToFloat() * dc.zoom, bbox.Height.ToFloat() * dc.zoom);

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
            bool hovered = CheckHovered(dc, dc.GetCursor());
            // 2. Create a Pen object
            Pen p = new Pen(Color.LightGreen, 3);

            // 3. Assign the custom cap to the start or end of the line
            if (IsSelected || hovered)
            {
                if (!dc.Editor.VirtualObjects.Contains(TargetAnchor))
                {
                    dc.Editor.VirtualObjects.Add(TargetAnchor);
                }
                if (!dc.Editor.VirtualObjects.Contains(SourceAnchor))
                {
                    dc.Editor.VirtualObjects.Add(SourceAnchor);
                }
            }
            else
            {
                if (dc.Editor.CurrentTool is not ArrowMoveByAnchorTool)
                {
                    if (dc.Editor.VirtualObjects.Contains(TargetAnchor))
                        dc.Editor.VirtualObjects.Remove(TargetAnchor);

                    if (dc.Editor.VirtualObjects.Contains(SourceAnchor))
                        dc.Editor.VirtualObjects.Remove(SourceAnchor);
                }
            }
            //dc.gr.DrawRectangle(new Pen(Color.LightGreen, 1), rect);
            if (IsSelected)
            {
                p = new Pen(Color.LightBlue, 3);

            }
            else if (hovered)
            {
                p = new Pen(Color.Red, 3);
                // dc.gr.DrawRectangle(new Pen(Color.Red, 3), rect);
            }
            p.CustomEndCap = myArrow;


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

            //dc.gr.DrawEllipse(Pens.Blue, from.X - cw / 2, from.Y - cw / 2, cw, cw);
            // dc.gr.DrawEllipse(Pens.Blue, to.X - cw / 2, to.Y - cw / 2, cw, cw);

        }

        public enum CurveTypeEnum
        {
            Line, Bezier
        }
        public CurveTypeEnum CurveType { get; set; }
        public void Event(IUIEvent ev)
        {
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
                    dd.AddOptionsField("curveType", "Curve type", ["line", "bezier"], 0);

                    if (!dd.ShowDialog())
                    {
                        ev.Handled = true;
                        return;
                    }


                    ZOrder = dd.GetNumericField("z");
                    CurveType = dd.GetOptionsFieldIdx("curveType") == 0 ? CurveTypeEnum.Line : CurveTypeEnum.Bezier;
                    ev.Handled = true;
                    return;
                }
            }
            else if (ev is UiMouseClickEvent mev)
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

        public XElement ToXml(ProjectXmlStoreContext ctx)
        {
            XElement ret = new XElement("item");
            ret.Add(new XAttribute("kind", "arrow"));



            ret.Add(new XAttribute("zOrder", ZOrder));
            ret.Add(new XAttribute("curveType", CurveType));

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
