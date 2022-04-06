namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct Propor : IClose<Propor>, IExponentiable<UDouble, Propor>
    {
        public static readonly Propor full = new(value: 1);
        public static readonly Propor empty = new(value: 0);

        public static Propor? Create(UDouble part, UDouble whole)
            => Create(propor: (double)(part / whole));

        public static Propor? Create(double propor)
        {
            double value = Snap(propor);
            if (value is >= 0 and <= 1)
                return new(value: value);
            return null;
        }

        private readonly UDouble value;

        private Propor(double value)
        {
            value = Snap(value);
            Debug.Assert(value is >= 0 and <= 1);
            this.value = (UDouble)value;
        }

        public Propor Opposite()
            => new(value: 1 - (double)value);

        public bool IsCloseTo(Propor other)
            => MyMathHelper.AreClose(value, other.value);

        public Propor Pow(UDouble exponent)
            => (Propor)MyMathHelper.Pow(value, (double)exponent);

        public static explicit operator Propor(double propor)
            => Create(propor: propor) switch
            {
                Propor proportion => proportion,
                null => throw new InvalidCastException()
            };

        public static explicit operator Propor(UDouble propor)
            => (Propor)(double)propor;

        public static explicit operator double(Propor propor)
            => (double)propor.value;

        public static explicit operator UDouble(Propor propor)
            => propor.value;

        public static Propor operator *(Propor propor1, Propor propor2)
            => (Propor)(propor1.value * propor2.value);

        // TODO: consider if this is needed or if implicit conversion to double and UDouble is better
        public static double operator *(Propor propor, double value)
            => (double)propor * value;

        public static double operator *(double value, Propor propor)
            => propor * value;

        public static UDouble operator *(Propor propor, UDouble value)
            => (UDouble)propor * value;

        public static UDouble operator *(UDouble value, Propor propor)
            => propor * value;

        public static TimeSpan operator *(Propor propor, TimeSpan timeSpan)
            => (double)propor * timeSpan;

        public static TimeSpan operator *(TimeSpan timeSpan, Propor propor)
            => propor * timeSpan;

        public static bool operator <=(Propor propor1, Propor propor2)
            => propor1.value <= propor2.value;

        public static bool operator >=(Propor propor1, Propor propor2)
            => propor1.value >= propor2.value;

        private static double Snap(double value)
        {
            if (MyMathHelper.AreClose(value, 0))
                value = 0;
            if (MyMathHelper.AreClose(value, 1))
                value = 1;
            return value;
        }
    }
}
