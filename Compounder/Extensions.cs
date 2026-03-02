using OpenTK.Mathematics;

namespace Compounder
{
    public static class Extensions
    {
        public static float ToFloat(this double d) => (float)d;
        public static Vector2d ToVector2d(this PointF d) => new Vector2d(d.X, d.Y);
    }
}