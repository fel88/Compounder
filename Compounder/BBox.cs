using OpenTK.Mathematics;

namespace Compounder
{
    public class BBox
    {
        public BBox() { }
        public BBox(Vector2d location, double width, double height)
        {
            Location = location;
            Width = width;
            Height = height;
        }
        public BBox(double x, double y, double width, double height)
        {
            Location = new Vector2d(x, y);
            Width = width;
            Height = height;
        }

        public bool Contains(Vector2d v)
        {
            return v.X >= Location.X && v.Y >= Location.Y && v.X <= (Location.X + Width) && v.Y <= (Location.Y + Height);
        }

        public Vector2d Location;
        public double Width;
        public double Height;
        public double Right => Location.X + Width;
        public double Bottom => Location.Y + Height;
        public double Area => Width * Height;
        public BBox Combine(BBox b)
        {
            var minx = Math.Min(Location.X, b.Location.X);
            var miny = Math.Min(Location.Y, b.Location.Y);
            var maxx = Math.Max(Right, b.Right);
            var maxy = Math.Max(Bottom, b.Bottom);
            return new BBox(minx, miny, maxx - minx, maxy - miny);
        }
    }
}
