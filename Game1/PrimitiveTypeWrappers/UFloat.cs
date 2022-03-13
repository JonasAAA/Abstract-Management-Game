using Microsoft.Xna.Framework;
using System;

namespace Game1.PrimitiveTypeWrappers
{
    // TODO: could change into UDouble
    public readonly struct UFloat
    {
        public static readonly UFloat pi;

        static UFloat()
            => pi = new(MathHelper.Pi);

        private readonly float value;

        private UFloat(float value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            this.value = value;
        }

        public static implicit operator float(UFloat UFloat)
            => UFloat.value;

        public static implicit operator UFloat(uint value)
            => new(value: value);

        public static implicit operator UFloat(ulong value)
            => new(value: value);

        public static explicit operator UFloat(float value)
            => new(value: value);

        public static explicit operator UFloat(double value)
            => new(value: (float)value);

        public static UFloat operator *(UFloat UFloatA, UFloat UFloatB)
            => new(UFloatA.value * UFloatB.value);

        public static UFloat operator *(uint UIntA, UFloat UFloatB)
            => new(UIntA * UFloatB.value);

        public static UFloat operator /(UFloat UFloatA, UFloat UFloatB)
           => new(UFloatA.value / UFloatB.value);
    }
}
