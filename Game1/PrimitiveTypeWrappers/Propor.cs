using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct Propor : IClose<Propor>, IMin<Propor>, IExponentiable<UDouble, Propor>, IComparisonOperators<Propor, Propor, bool>,
        IMultiplyOperators<Propor, Propor, Propor>, IMultiplicativeIdentity<Propor, Propor>,
        IMultiplyOperators<Propor, double, double>,
        IMultiplyOperators<Propor, UDouble, UDouble>,
        IMultiplyOperators<Propor, TimeSpan, TimeSpan>
    {
        public static readonly Propor full = new(value: 1);
        public static readonly Propor empty = new(value: 0);

        static Propor IMultiplicativeIdentity<Propor, Propor>.MultiplicativeIdentity
            => full;

        public static Propor? Create(double part, double whole)
            => Create(value: part / whole);

        public static Propor? Create(UDouble part, UDouble whole)
        {
            if (whole == 0)
                return null;
            return Create(value: part / whole);
        }

        public static Propor? Create(double value)
        {
            if (double.IsNaN(value))
                return null;
            if (value < 0 && MyMathHelper.AreClose(value, 0))
                value = 0;
            if (value > 1 && MyMathHelper.AreClose(value, 1))
                value = 1;
            if (value is >= 0 and <= 1)
                return new(value: value);
            return null;
        }

        public static Propor CreateByClamp(UDouble value)
            => new(value: MyMathHelper.Min((double)value, 1));

        public bool IsFull
            => this == full;

        public bool IsEmpty
            => this == empty;

        private readonly UDouble value;

        private Propor(double value)
        {
            Debug.Assert(value is >= 0 and <= 1);
            this.value = (UDouble)value;
        }

        public Propor Opposite()
            => (Propor)(1 - value);

        public bool IsCloseTo(Propor other)
            => MyMathHelper.AreClose(value, other.value);

        public Propor Pow(UDouble exponent)
            => (Propor)MyMathHelper.Pow(value, exponent);

        public static explicit operator Propor(double propor)
            => Create(value: propor) switch
            {
                Propor proportion => proportion,
                null => throw new InvalidCastException()
            };

        public static explicit operator Propor(UDouble propor)
            => (Propor)(double)propor;

        public static explicit operator double(Propor propor)
            => propor.value;

        public static explicit operator UDouble(Propor propor)
            => propor.value;

        public static explicit operator decimal(Propor propor)
            => (decimal)(double)propor.value;

        public static Propor operator *(Propor propor1, Propor propor2)
            => (Propor)(propor1.value * propor2.value);

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

        public static Color operator *(Propor propor, Color value)
            => (float)propor * value;

        public static Color operator *(Color value, Propor propor)
            => propor * value;

        public static AreaDouble operator *(Propor propor, AreaDouble value)
            => AreaDouble.CreateFromMetSq(valueInMetSq: propor * value.valueInMetSq);

        public static AreaDouble operator *(AreaDouble value, Propor propor)
            => propor * value;

        public static bool operator >(Propor left, Propor right)
            => left.value > right.value;

        public static bool operator >=(Propor left, Propor right)
            => left.value >= right.value;

        public static bool operator <(Propor left, Propor right)
            => left.value < right.value;

        public static bool operator <=(Propor left, Propor right)
            => left.value <= right.value;

        public override string ToString()
            => $"{value:0.00}";

        static Propor IMin<Propor>.Min(Propor left, Propor right)
            => left < right ? left : right;
    }
}
