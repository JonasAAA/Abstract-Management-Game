namespace Game1.PrimitiveTypeWrappers
{
    // TODO: could rename to MyUFloat
    [Serializable]
    public readonly struct UDouble : IClose<UDouble>, IExponentiable<double, UDouble>, IMinable<UDouble>, IMaxable<UDouble>
    {
        public static readonly UDouble positiveInfinity = new(value: double.PositiveInfinity);

        public static UDouble? Create(double value)
        {
            if (value < 0 && MyMathHelper.AreClose(value, 0))
                value = 0;
            return value >= 0 ? new(value: value) : null;
        }

        private readonly double value;

        private UDouble(double value)
        {
            Debug.Assert(value >= 0);
            this.value = value;
        }

        public bool IsCloseTo(UDouble other)
            => MyMathHelper.AreClose(value, other.value);

        public UDouble Pow(double exponent)
            => (UDouble)MyMathHelper.Pow(value, exponent);

        public static bool operator <(UDouble value1, UDouble value2)
            => value1.value < value2.value;

        public static bool operator >(UDouble value1, UDouble value2)
            => value1.value > value2.value;

        public static bool operator <=(UDouble value1, UDouble value2)
            => value1.value <= value2.value;

        public static bool operator >=(UDouble value1, UDouble value2)
            => value1.value >= value2.value;

        // TODO: consider making conversion implicit
        public static explicit operator double(UDouble value)
            => value.value;

        public static implicit operator UDouble(uint value)
            => new(value: value);

        public static implicit operator UDouble(ulong value)
            => new(value: value);

        public static explicit operator UDouble(double value)
            => Create(value: value) switch
            {
                UDouble UDouble => UDouble,
                null => throw new InvalidCastException()
            };

        public static UDouble operator +(UDouble value1, UDouble value2)
            => new(value1.value + value2.value);

        public static UDouble operator *(UDouble value1, UDouble value2)
            => new(value1.value * value2.value);

        public static TimeSpan operator *(UDouble scale, TimeSpan timeSpan)
            => scale.value * timeSpan;

        public static TimeSpan operator *(TimeSpan timeSpan, UDouble scale)
            => scale * timeSpan;

        public static UDouble operator /(UDouble value1, UDouble value2)
           => new(value1.value / value2.value);

        public override string ToString()
            => value.ToString();

        UDouble IMinable<UDouble>.Min(UDouble other)
            => new(MyMathHelper.Min(value, other.value));

        UDouble IMaxable<UDouble>.Max(UDouble other)
            => new(MyMathHelper.Max(value, other.value));
    }
}
