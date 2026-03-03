using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Compounder
{
    public class ConnectorPoint : AbstractSceneObject, ISceneObject
    {
        public Vector2d RelativePositon;
        public ISceneObject Parent;


        public Vector2d Location => RelativePositon + (Parent == null ? new Vector2d() : Parent.Location);
        public double Width = 10;
        public double Height = 10;
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

            dc.gr.DrawRectangle(new Pen(Color.Black, 3), rect);

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
            return;
        }

        public BBox GetBBox()
        {
            return new BBox(Location.X, Location.Y, 1, 1);
        }

        public XElement ToXml()
        {
            return null;
        }
    }
}
