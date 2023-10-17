namespace Game1.MyMath
{
    /// <summary>
    /// Used for world and map creation stuff
    /// </summary>
    [Serializable]
    public readonly record struct MyVector2 : IEquatable<MyVector2>
    {
        public static readonly MyVector2 zero = new(xAndY: SignedLength.zero);

        // These are properties so that non-destructive mutation (i.e. vector with { X = 10 }) works
        public SignedLength X { get; init; }
        public SignedLength Y { get; init; }

        public MyVector2(SignedLength xAndY)
            : this(x: xAndY, y: xAndY)
        { }

        public MyVector2(SignedLength x, SignedLength y)
        {
            X = x;
            Y = y;
        }

        public Length Length()
            => PrimitiveTypeWrappers.Length.CreateFromM
            (
                valueInM: MyMathHelper.Sqrt
                (
                    MyMathHelper.Square(X.valueInM) + MyMathHelper.Square(Y.valueInM)
                )
            );

        public static SignedLength Dot(MyVector2 value1, Vector2Bare value2)
            => value1.X * value2.X + value1.Y * value2.Y;

        public static Length Distance(MyVector2 value1, MyVector2 value2)
            => (value1 - value2).Length();

        public static Vector2Bare Normalized(MyVector2 value)
        {
            var length = value.Length();
            if (length.IsTiny())
                throw new ArgumentException();
            return value / length;
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

        public static Vector2Bare operator /(MyVector2 vector, SignedLength scalar)
            => new(x: vector.X / scalar, y: vector.Y / scalar);

        public static explicit operator Vector2(MyVector2 myVector2)
            => new(x: (float)myVector2.X.valueInM, y: (float)myVector2.Y.valueInM);

        public static explicit operator MyVector2(Vector2 vector2)
            => new(x: SignedLength.CreateFromM(vector2.X), y: SignedLength.CreateFromM(vector2.Y));

        public static explicit operator Point(MyVector2 myVector2)
            => new(Convert.ToInt32(myVector2.X), Convert.ToInt32(myVector2.Y));

        public static explicit operator MyVector2(Point point)
            => new(x: SignedLength.CreateFromM(point.X), y: SignedLength.CreateFromM(point.Y));
    }
}
