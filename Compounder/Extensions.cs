using OpenTK.Mathematics;
using System.Globalization;

namespace Compounder
{
    public static class Extensions
    {
        public static double ToDouble(this string str) => double.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        public static float ToFloat(this double d) => (float)d;
        public static Vector2d ToVector2d(this PointF d) => new Vector2d(d.X, d.Y);
    }
}