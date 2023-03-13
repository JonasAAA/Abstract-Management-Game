using Game1.ContentHelpers;

namespace Game1.MyMath
{
    [Serializable]
    public readonly record struct MyVector2 : IEquatable<MyVector2>
    {
        public static readonly MyVector2 zero = new(xAndY: 0);

        public double X { get; init; }
        public double Y { get; init; }

        public MyVector2(MyVector2Info vector2Info)
            : this(x: vector2Info.X, y: vector2Info.Y)
        { }

        public MyVector2(double xAndY)
            : this(x: xAndY, y: xAndY)
        { }

        public MyVector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public UDouble LengthSquared()
            => MyMathHelper.Square(X) + MyMathHelper.Square(Y);

        public UDouble Length()
            => MyMathHelper.Sqrt(LengthSquared());

        public static double Dot(MyVector2 value1, MyVector2 value2)
            => value1.X * value2.X + value1.Y * value2.Y;

        public static UDouble Distance(MyVector2 value1, MyVector2 value2)
            => (value1 - value2).Length();

        public static MyVector2 Transform(MyVector2 position, Matrix matrix)
            => (MyVector2)Vector2.Transform((Vector2)position, matrix);

        public static MyVector2 Normalized(MyVector2 value)
        {
            var length = value.Length();
            if (MyMathHelper.IsTiny(value: length))
                throw new ArgumentException();
            return value / value.Length();
        }

        public static MyVector2 operator +(MyVector2 value)
            => value;

        public static MyVector2 operator -(MyVector2 value)
            => new(x: -value.X, y: -value.Y);

        public static MyVector2 operator +(MyVector2 value1, MyVector2 value2)
            => new(x: value1.X + value2.X, y: value1.Y + value2.Y);

        public static MyVector2 operator -(MyVector2 value1, MyVector2 value2)
            => value1 + (-value2);

        public static MyVector2 operator *(double scalar, MyVector2 vector)
            => new(x: scalar * vector.X, y: scalar * vector.Y);

        public static MyVector2 operator *(MyVector2 vector, double scalar)
            => scalar * vector;

        public static MyVector2 operator /(MyVector2 vector, double scalar)
            => vector * (1 / scalar);

        public static explicit operator Vector2(MyVector2 myVector2)
            => new(x: (float)myVector2.X, y: (float)myVector2.Y);

        public static explicit operator MyVector2(Vector2 vector2)
            => new(x: vector2.X, y: vector2.Y);

        public static explicit operator Point(MyVector2 myVector2)
            => new(Convert.ToInt32(myVector2.X), Convert.ToInt32(myVector2.Y));

        public static explicit operator MyVector2(Point point)
            => new(x: point.X, y: point.Y);
    }
}
