namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct Temperature : IComparable<Temperature>, IScalar<Temperature>
    {
        public static readonly Temperature zero = new(valueInK: 0);

        public static Temperature CreateFromK(UDouble valueInK)
            => new(valueInK: valueInK);

        // This must be property rather than field so that auto-initialized Temperature IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UDouble valueInK;

        private Temperature(UDouble valueInK)
            => this.valueInK = valueInK;

        public override string ToString()
            => $"{valueInK:#,0.} K";

        public static bool operator <=(Temperature left, Temperature right)
            => left.valueInK <= right.valueInK;

        public static bool operator >=(Temperature left, Temperature right)
            => left.valueInK >= right.valueInK;

        int IComparable<Temperature>.CompareTo(Temperature other)
            => valueInK.CompareTo(other.valueInK);

        public static Propor Normalize(Temperature value, Temperature start, Temperature stop)
            => UDouble.Normalize(value: value.valueInK, start: start.valueInK, stop: stop.valueInK);

        public static Temperature Interpolate(Propor normalized, Temperature start, Temperature stop)
            => new(valueInK: UDouble.Interpolate(normalized: normalized, start: start.valueInK, stop: stop.valueInK));
    }
}
