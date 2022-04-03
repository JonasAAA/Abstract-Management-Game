namespace Game1.PrimitiveTypeWrappers
{
    // TODO: could change into UDouble
    [Serializable]
    public readonly struct UFloat : IMinable<UFloat>, IMaxable<UFloat>
    {
        public static readonly UFloat pi;

        static UFloat()
            => pi = new(MathHelper.pi);

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

        public static UFloat operator +(UFloat UFloat1, UFloat UFloat2)
            => new(UFloat1.value + UFloat2.value);

        public static UFloat operator *(UFloat UFloatA, UFloat UFloatB)
            => new(UFloatA.value * UFloatB.value);

        public static UFloat operator /(UFloat UFloatA, UFloat UFloatB)
           => new(UFloatA.value / UFloatB.value);

        public override string ToString()
            => value.ToString();

        UFloat IMinable<UFloat>.Min(UFloat other)
            => new(MathHelper.Min(value, other.value));

        UFloat IMaxable<UFloat>.Max(UFloat other)
            => new(MathHelper.Max(value, other.value));
    }
}
