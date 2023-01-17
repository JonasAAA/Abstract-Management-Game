using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    // TODO: could rename to MyUFloat
    [Serializable]
    public readonly struct UDouble : IClose<UDouble>, IExponentiable<double, UDouble>, IComparisonOperators<UDouble, UDouble, bool>, IComparable<UDouble>, IMinMaxValue<UDouble>, IAdditionOperators<UDouble, UDouble, UDouble>, IAdditiveIdentity<UDouble, UDouble>, IMultiplyOperators<UDouble, UDouble, UDouble>, IMultiplicativeIdentity<UDouble, UDouble>, IPrimitiveTypeWrapper
    {
        public static readonly UDouble positiveInfinity = new(value: double.PositiveInfinity);

        public static readonly UDouble zero = 0;

        static UDouble IAdditiveIdentity<UDouble, UDouble>.AdditiveIdentity
            => zero;

        static UDouble IMultiplicativeIdentity<UDouble, UDouble>.MultiplicativeIdentity
            => 1;

        /// <summary>
        /// Note that this is maximum possible double value, not positive infinity!
        /// </summary>
        static UDouble IMinMaxValue<UDouble>.MaxValue
            => new(value: double.MaxValue);

        static UDouble IMinMaxValue<UDouble>.MinValue
            => zero;

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

        public static bool operator ==(UDouble value1, UDouble value2)
            => value1.value == value2.value;

        public static bool operator !=(UDouble value1, UDouble value2)
            => value1.value != value2.value;

        public static bool operator <(UDouble value1, UDouble value2)
            => value1.value < value2.value;

        public static bool operator >(UDouble value1, UDouble value2)
            => value1.value > value2.value;

        public static bool operator <=(UDouble value1, UDouble value2)
            => value1.value <= value2.value;

        public static bool operator >=(UDouble value1, UDouble value2)
            => value1.value >= value2.value;

        public static implicit operator double(UDouble value)
            => value.value;

        public static explicit operator decimal(UDouble value)
            => (decimal)value.value;

        public static explicit operator ulong(UDouble value)
            => (ulong)value.value;

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

        public string ToString(string? format, IFormatProvider? formatProvider)
            => value.ToString(format, formatProvider);

        public int CompareTo(UDouble other)
            => value.CompareTo(other.value);

        public override bool Equals(object? obj)
            => obj is UDouble UDouble && value == UDouble.value;

        public override int GetHashCode()
            => value.GetHashCode();
    }
}
