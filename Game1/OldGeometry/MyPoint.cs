using ClipperLib;
using LibTessDotNet;

namespace Game1.OldGeometry
{
    [Serializable]
    public readonly struct MyPoint
    {
        // power of two to make float / scale and float * scale lossless
        private const long scale = (long)1 << 31;

        private readonly long x, y;

        private MyPoint(long x, long y)
        {
            this.x = x;
            this.y = y;
        }

        // on implicit type conversion https://softwareengineering.stackexchange.com/questions/284359/when-should-i-use-cs-implicit-type-conversion-operator
        public static explicit operator IntPoint(MyPoint myPoint)
            => new(myPoint.x, myPoint.y);

        public static explicit operator MyPoint(IntPoint intPoint)
            => new(intPoint.X, intPoint.Y);

        public static explicit operator Vector3(MyPoint myPoint)
            => new((float)myPoint.x / scale, (float)myPoint.y / scale, 0);

        public static explicit operator MyPoint(Vector3 vector3)
            => new(Convert.ToInt64(vector3.X * scale), Convert.ToInt64(vector3.Y * scale));

        public static explicit operator Vec3(MyPoint myPoint)
            => new((float)myPoint.x / scale, (float)myPoint.y / scale, 0);

        public static explicit operator MyPoint(Vec3 vec3)
            => new(Convert.ToInt64(vec3.X * scale), Convert.ToInt64(vec3.Y * scale));
    }
}
