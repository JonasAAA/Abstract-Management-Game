namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct Temperature : IComparable<Temperature>
    {
        public static readonly Temperature zero;

        static Temperature()
            => zero = new(valueInK: 0);

        public static Temperature CreateFromK(UDouble valueInK)
            => new(valueInK: valueInK);

        // This must be property rather than field so that auto-initialized Temperature IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UDouble valueInK;

        private Temperature(UDouble valueInK)
            => this.valueInK = valueInK;

        public override string ToString()
            => $"{valueInK} K";

        public static bool operator <=(Temperature left, Temperature right)
            => left.valueInK <= right.valueInK;

        public static bool operator >=(Temperature left, Temperature right)
            => left.valueInK >= right.valueInK;

        int IComparable<Temperature>.CompareTo(Temperature other)
            => valueInK.CompareTo(other.valueInK);
    }
}
