﻿using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    // TODO: could rename to MyUFloat
    [Serializable]
    public readonly struct UDouble : IFormattable, IScalar<UDouble>, IClose<UDouble>, IExponentiable<double, UDouble>,
        IComparisonOperators<UDouble, UDouble, bool>, IMin<UDouble>, IMax<UDouble>, IComparable<UDouble>, IEquatable<UDouble>, IMinMaxValue<UDouble>,
        IAdditionOperators<UDouble, UDouble, UDouble>, IAdditiveIdentity<UDouble, UDouble>,
        ISubtractionOperators<UDouble, UDouble, UDouble>,
        IMultiplyOperators<UDouble, UDouble, UDouble>, IMultiplicativeIdentity<UDouble, UDouble>,
        IMultiplyOperators<UDouble, ulong, UDouble>, IMultiplicativeIdentity<UDouble, ulong>
    {
        public static readonly UDouble zero = 0;

        public static readonly UDouble half = new(value: .5);

        public static readonly UDouble positiveInfinity = new(value: double.PositiveInfinity);

        static UDouble IAdditiveIdentity<UDouble, UDouble>.AdditiveIdentity
            => zero;

        static UDouble IMultiplicativeIdentity<UDouble, UDouble>.MultiplicativeIdentity
            => 1;

        static ulong IMultiplicativeIdentity<UDouble, ulong>.MultiplicativeIdentity
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

        public static UDouble CreateByClamp(double value)
            => new(value: MyMathHelper.Max(value, 0));

        private readonly double value;

        private UDouble(double value)
        {
            Debug.Assert(value >= 0);
            this.value = value;
        }

        public bool IsTiny()
            => IsCloseTo(zero);

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

        public static explicit operator float(UDouble value)
            => (float)value.value;

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

        public static UDouble operator -(UDouble value1, UDouble value2)
            => new(value1.value - value2.value);

        public static UDouble operator *(UDouble value1, UDouble value2)
            => new(value1.value * value2.value);

        public static UDouble operator *(UDouble value1, ulong value2)
            => new(value1.value * value2);

        public static TimeSpan operator *(UDouble scale, TimeSpan timeSpan)
            => scale.value * timeSpan;

        public static TimeSpan operator *(TimeSpan timeSpan, UDouble scale)
            => scale * timeSpan;

        public static UDouble operator /(UDouble value1, UDouble value2)
        {
            if (value2.value is 0)
                throw new ArithmeticException("Dividend cannot be 0");
            return new(value1.value / value2.value);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
            => value.ToString(format, formatProvider);

        public int CompareTo(UDouble other)
            => value.CompareTo(other.value);

        public override bool Equals(object? obj)
            => obj is UDouble UDouble && value == UDouble.value;

        bool IEquatable<UDouble>.Equals(UDouble UDouble)
            => value == UDouble.value;

        public override int GetHashCode()
            => value.GetHashCode();

        static UDouble IMin<UDouble>.Min(UDouble left, UDouble right)
            => left < right ? left : right;

        static UDouble IMax<UDouble>.Max(UDouble left, UDouble right)
            => left > right ? left : right;

        public static Propor Normalize(UDouble value, UDouble start, UDouble stop)
            => Algorithms.Normalize(value: value.value, start: start, stop: stop);

        public static UDouble Interpolate(Propor normalized, UDouble start, UDouble stop)
            => new(value: Algorithms.Interpolate(normalized: normalized, start: (double)start, stop: stop));
    }
}
