using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Compounder
{
    public class ArrowMoveAnchor : AbstractSceneObject, ISceneObject
    {
        public ArrowMoveAnchor(ArrowSceneObject parent, Action<ArrowMoveAnchor> locationChanged)
        {
            Parent = parent;
            LocationChangedAction = locationChanged;
        }
        protected ArrowSceneObject Parent;

        public Action<ArrowMoveAnchor> LocationChangedAction;
        Vector2d _location;
        public Vector2d Location
        {
            get => _location;
            set
            {
                _location = value;
                LocationChangedAction?.Invoke(this);
            }
        }

        public override double ZOrder => int.MaxValue;
        public bool CheckHovered(DrawingContext dc, CursorPosition location)
        {
            var t0 = dc.Transform(Location);

            var rect = new RectangleF(t0.X - AnchorWidth / 2, t0.Y - AnchorWidth / 2, AnchorWidth, AnchorWidth);
            return rect.Contains(location.Screen.ToPointF());
        }

        public const float AnchorWidth = 15;
        public const float AnchorCrossGap = 2;
        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);

            var rect = new RectangleF(t0.X - AnchorWidth / 2, t0.Y - AnchorWidth / 2, AnchorWidth, AnchorWidth);

            Color fillColor = Color.LightBlue;
            if (IsSelected)
            {
                fillColor = Color.LightGreen;
            }
            else if (CheckHovered(dc, dc.GetCursor()) || dc.Editor.CurrentTool is MoveByAnchorTool)
            {
                fillColor = Color.RebeccaPurple;
            }
            dc.gr.FillEllipse(new SolidBrush(fillColor), rect);
            dc.gr.DrawLine(Pens.Blue, rect.X + rect.Width / 2, rect.Y + AnchorCrossGap, rect.X + rect.Width / 2, rect.Bottom - AnchorCrossGap);
            dc.gr.DrawLine(Pens.Blue, rect.X + AnchorCrossGap, rect.Y + rect.Height / 2, rect.Right - AnchorCrossGap, rect.Y + rect.Height / 2);
            dc.gr.DrawEllipse(new Pen(Color.Black, 2), rect);

        }

        public void Event(IUIEvent ev)
        {
            var dc = ev.DrawingContext;
            if (ev is UiMouseMoveEvent mmv)
            {
                IsHovered = CheckHovered(dc, mmv.Location);
            }
            else
            if (ev is UiMouseClickEvent mev)
            {
                if (mev.Button == MouseButtons.Left)
                {
                    if (CheckHovered(dc, mev.Location))
                    {
                        mev.Editor.SetTool(new ArrowMoveByAnchorTool(Parent, this, mev.Editor, mev.DrawingContext));
                        ev.Handled = true;
                    }
                    return;
                }
            }
        }

        public BBox GetBBox()
        {
            return new BBox(Location.X, Location.Y - AnchorWidth, AnchorWidth, AnchorWidth);
        }


        public XElement ToXml()
        {
            return null;
        }
    }
}
