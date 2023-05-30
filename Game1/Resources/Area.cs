using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Area : IOrderedVector<Area, ulong>
    {
        public static readonly Area zero;

        static Area IAdditiveIdentity<Area, Area>.AdditiveIdentity
            => zero;

        static ulong IMultiplicativeIdentity<Area, ulong>.MultiplicativeIdentity
            => 1;

        static Area()
            => zero = new(valueInMetSq: 0);

        public static Area CreateFromMetSq(ulong valueInMetSq)
            => new(valueInMetSq: valueInMetSq);

        // This must be property rather than field so that auto-initialized Area IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly ulong valueInMetSq;

        private Area(ulong valueInMetSq)
            => this.valueInMetSq = valueInMetSq;

        public override string ToString()
            => $"{valueInMetSq} m^2";

        public static Area operator +(Area left, Area right)
            => new(valueInMetSq: left.valueInMetSq + right.valueInMetSq);

        public static Area operator -(Area left, Area right)
            => new(valueInMetSq: left.valueInMetSq - right.valueInMetSq);

        public static Area operator *(Area left, ulong right)
            => new(valueInMetSq: left.valueInMetSq * right);

        public static Area operator *(ulong left, Area right)
            => right * left;

        public static bool operator <=(Area left, Area right)
            => left.valueInMetSq <= right.valueInMetSq;

        public static bool operator >=(Area left, Area right)
            => left.valueInMetSq >= right.valueInMetSq;

        public static bool operator <(Area left, Area right)
            => left.valueInMetSq < right.valueInMetSq;

        public static bool operator >(Area left, Area right)
            => left.valueInMetSq > right.valueInMetSq;
    }
}
