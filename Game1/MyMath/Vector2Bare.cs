using System.Numerics;

namespace Game1.MyMath
{
    /// <summary>
    /// I.e. with no units
    /// Used for screen position and HUD position, and things like normalized vectors
    /// </summary>
    [Serializable]
    public readonly struct Vector2Bare :
        IAdditionOperators<Vector2Bare, Vector2Bare, Vector2Bare>,
        IMultiplyOperators<Vector2Bare, SignedLength, MyVector2>,
        IMultiplyOperators<Vector2Bare, double, Vector2Bare>,
        IDivisionOperators<Vector2Bare, double, Vector2Bare>,
        ISubtractionOperators<Vector2Bare, Vector2Bare, Vector2Bare>
    {
        public static readonly Vector2Bare zero = new(x: 0, y: 0);

        // These are properties so that non-destructive mutation (i.e. vector with { X = 10 }) works
        public double X { get; init; }
        public double Y { get; init; }

        public Vector2Bare(double xAndY)
            : this(x: xAndY, y: xAndY)
        { }

        public Vector2Bare(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static double Dot(Vector2Bare value1, Vector2Bare value2)
            => value1.X * value2.X + value1.Y * value2.Y;

        public UDouble LengthSquared()
            => MyMathHelper.Square(X) + MyMathHelper.Square(Y);

        public UDouble Length()
            => MyMathHelper.Sqrt(LengthSquared());

        public static UDouble Distance(Vector2Bare value1, Vector2Bare value2)
            => (value1 - value2).Length();

        public static Vector2Bare Normalized(Vector2Bare value)
        {
            var length = value.Length();
            if (length.IsTiny())
                throw new ArgumentException();
            return value / length;
        }

        public static Vector2Bare operator -(Vector2Bare left, Vector2Bare right)
            => new(left.X - right.X, left.Y - right.Y);

        public static Vector2Bare operator +(Vector2Bare left, Vector2Bare right)
            => new(left.X + right.X, left.Y + right.Y);

        public static MyVector2 operator *(Vector2Bare vector, SignedLength scalar)
            => new(x: vector.X * scalar, y: vector.Y * scalar);

        public static MyVector2 operator *(SignedLength scalar, Vector2Bare vector)
            => vector * scalar;

        public static Vector2Bare operator *(Vector2Bare vector, double scalar)
            => new(x: vector.X * scalar, y: vector.Y * scalar);

        public static Vector2Bare operator *(double scalar, Vector2Bare vector)
            => vector * scalar;

        public static Vector2Bare operator /(Vector2Bare vector, double scalar)
            => new(x: vector.X / scalar, y: vector.Y / scalar);

        public static explicit operator Microsoft.Xna.Framework.Vector2(Vector2Bare vector2)
            => new(x: (float)vector2.X, y: (float)vector2.Y);

        public static explicit operator Vector2Bare(Microsoft.Xna.Framework.Vector2 vector2)
            => new(x: vector2.X, y: vector2.Y);

        public static explicit operator Point(Vector2Bare myVector2)
            => new(Convert.ToInt32(myVector2.X), Convert.ToInt32(myVector2.Y));

        public static explicit operator Vector2Bare(Point point)
            => new(x: point.X, y: point.Y);
    }
}
