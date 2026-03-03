using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Linq;

namespace Compounder
{
    public class MoveAnchor : AbstractSceneObject, ISceneObject
    {
        public Vector2d Location { get; set; }

        public override double ZOrder => int.MaxValue;
        public bool CheckHovered(DrawingContext dc, CursorPosition location)
        {
            var t0 = dc.Transform(Location);

            var rect = new RectangleF(t0.X - AnchorWidth, t0.Y - AnchorWidth, AnchorWidth, AnchorWidth);
            return rect.Contains(location.Screen.ToPointF());
        }

        public const float AnchorWidth = 20;
        public const float AnchorCrossGap = 2;
        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);

            var rect = new RectangleF(t0.X - AnchorWidth, t0.Y - AnchorWidth, AnchorWidth, AnchorWidth);

            Color fillColor = Color.Pink;
            if (IsSelected)
            {
                fillColor = Color.LightGreen;
            }
            else if (CheckHovered(dc, dc.GetCursor()) || dc.Editor.CurrentTool is MoveByAnchorTool)
            {
                fillColor = Color.RebeccaPurple;
            }
            dc.gr.FillRectangle(new SolidBrush(fillColor), rect);
            dc.gr.DrawLine(Pens.Black, rect.X + rect.Width / 2, rect.Y + AnchorCrossGap, rect.X + rect.Width / 2, rect.Bottom - AnchorCrossGap);
            dc.gr.DrawLine(Pens.Black, rect.X + AnchorCrossGap, rect.Y + rect.Height / 2, rect.Right - AnchorCrossGap, rect.Y + rect.Height / 2);
            dc.gr.DrawRectangle(new Pen(Color.Black, 2), rect);

        }

        public void Event(IUIEvent ev)
        {
            var dc = ev.DrawingContext;
            if (ev is UiMouseEvent mev)
            {
                if (mev.Button == MouseButtons.Left)
                {
                    if (CheckHovered(dc, mev.Location))
                    {
                        mev.Editor.SetTool(new MoveByAnchorTool(this, mev.Editor, mev.DrawingContext));
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

    public struct CursorPosition
    {
        public Vector2d World;
        public Vector2d Screen;
    }
}
