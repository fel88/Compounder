using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Windows.Forms;

namespace Compounder
{
    public class RectCreationTool : AbstractTool, ITool
    {
        public RectCreationTool(IEditor editor, IDrawingContext dc) : base(editor, dc)
        {



        }


        public void Deselect()
        {

        }

        public void Draw(IDrawingContext dc)
        {
            if (curs.Count == 0)
                return;

            var curp = dc.GetCursor();
            var arr = new Vector2d[] { curp.World, curs[0].World };
            var minx = arr.Min(z => z.X);
            var miny = arr.Min(z => z.Y);
            var maxx = arr.Max(z => z.X);
            var maxy = arr.Max(z => z.Y);
            var w = maxx - minx;
            var h = maxy - miny;

            var temp = new RectObject()
            {
                Width = w,
                Height = h,
                Text = string.Empty,
                Location = new Vector2d(minx, miny + h),
                Fill = true
            };

            temp.Draw(dc as DrawingContext);
        }

        public void MouseDown(UiMouseClickEvent mev)
        {
            if (mev.Button != MouseButtons.Left)
                return;
            curs.Add(mev.Location);
        }
        List<CursorPosition> curs = new List<CursorPosition>();
        public void MouseUp(UiMouseClickEvent mev)
        {
            if (mev.Button != MouseButtons.Left)
                return;
            curs.Add(mev.Location);
            if (curs.Count == 2)
            {
                var minx = curs.Min(z => z.World.X);
                var miny = curs.Min(z => z.World.Y);
                var maxx = curs.Max(z => z.World.X);
                var maxy = curs.Max(z => z.World.Y);
                var w = maxx - minx;
                var h = maxy - miny;
                Editor.Project.Objects.Add(new RectObject()
                {
                    Width = w,
                    Height = h,
                    Text = "rect01",
                    Location = new Vector2d(minx, miny + h)
                });
                Editor.ResetTool();
            }
        }

        public void Select()
        {

        }
    }
}
