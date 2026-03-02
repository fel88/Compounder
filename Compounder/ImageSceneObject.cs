using OpenTK.Mathematics;

namespace Compounder
{
    public class ImageSceneObject : AbstractSceneObject, ISceneObject
    {
        public Bitmap Bitmap;
        public Vector2d Location { get; set; }

        public bool CheckHovered(DrawingContext dc, Vector2d location)
        {
            var rect = new RectangleF(Location.X.ToFloat(), Location.Y.ToFloat() - Bitmap.Height, Bitmap.Width, Bitmap.Height);
            return rect.Contains(location.X.ToFloat(), location.Y.ToFloat());
        }

        public void Draw(DrawingContext dc)
        {
            var t0 = dc.Transform(Location);
            var rect = new RectangleF(t0.X, t0.Y, Bitmap.Width * dc.zoom * ImgScale.ToFloat(), Bitmap.Height * dc.zoom * ImgScale.ToFloat());

            dc.gr.DrawImage(Bitmap, rect);
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
        public void Event(DrawingContext dc, IUIEvent ev)
        {
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
                    if (!dd.ShowDialog())
                    {
                        ev.Handled = true;
                        return;
                    }

                    ImgScale = dd.GetNumericField("scale");
                    ZOrder = dd.GetNumericField("z");
                    ev.Handled = true;
                    return;
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
    }
}
