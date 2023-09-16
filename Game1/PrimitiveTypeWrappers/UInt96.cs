using System;
using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    // Why 96? Need more than 64 bits, and also that can accurately compute stuff like num * 2.356.
    // In this case, will use decimal for that, which has 96 bit integer precision, so that's where 96 comes from.
    [Serializable]
    public readonly struct UInt96 : IComparisonOperators<UInt96, UInt96, bool>, IMin<UInt96>, IMax<UInt96>, IComparable<UInt96>, IEquatable<UInt96>, IMinMaxValue<UInt96>,
        IAdditionOperators<UInt96, UInt96, UInt96>, IAdditiveIdentity<UInt96, UInt96>,
        ISubtractionOperators<UInt96, UInt96, UInt96>,
        IMultiplyOperators<UInt96, UInt96, UInt96>, IMultiplicativeIdentity<UInt96, UInt96>,
        IPrimitiveTypeWrapper
    {
        public static readonly UInt96 maxValue = new(new(upper: uint.MaxValue, lower: ulong.MaxValue));

        static UInt96 IAdditiveIdentity<UInt96, UInt96>.AdditiveIdentity
            => 0;

        static UInt96 IMultiplicativeIdentity<UInt96, UInt96>.MultiplicativeIdentity
            => 1;

        private readonly UInt128 value;

        private UInt96(UInt128 value)
            => this.value = value;

        public static implicit operator UInt96(uint value)
            => new(value);

        public static explicit operator UInt96(decimal value)
            => new((UInt128)value);

        public static implicit operator decimal(UInt96 value)
            => (decimal)value.value;

        public static UInt96 operator +(UInt96 left, UInt96 right)
            => new(left.value + right.value);

        public static UInt96 operator -(UInt96 left, UInt96 right)
            => new(left.value - right.value);

        public static UInt96 operator *(UInt96 left, UInt96 right)
            => new(left.value * right.value);

        public static bool operator ==(UInt96 left, UInt96 right)
            => left.value == right.value;

        public static bool operator !=(UInt96 left, UInt96 right)
            => left.value != right.value;

        public static bool operator <(UInt96 left, UInt96 right)
            => left.value < right.value;

        public static bool operator >(UInt96 left, UInt96 right)
            => left.value > right.value;

        public static bool operator <=(UInt96 left, UInt96 right)
            => left.value <= right.value;

        public static bool operator >=(UInt96 left, UInt96 right)
            => left.value >= right.value;
    }
}
