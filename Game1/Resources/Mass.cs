using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Mass : IOrderedVector<Mass, UInt96>
    {
        public static readonly Mass zero = new(valueInKg: 0);

        static Mass IAdditiveIdentity<Mass, Mass>.AdditiveIdentity
            => zero;

        static UInt96 IMultiplicativeIdentity<Mass, UInt96>.MultiplicativeIdentity
            => 1;

        public static Mass CreateFromKg(UInt96 valueInKg)
            => new(valueInKg: valueInKg);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UInt96 valueInKg;

        private Mass(UInt96 valueInKg)
            => this.valueInKg = valueInKg;

        public override string ToString()
            => $"{valueInKg} Kg";

        public static Mass operator +(Mass left, Mass right)
            => new(valueInKg: left.valueInKg + right.valueInKg);

        public static Mass operator -(Mass left, Mass right)
            => new(valueInKg: left.valueInKg - right.valueInKg);

        public static Mass operator *(Mass left, UInt96 right)
            => new(valueInKg: left.valueInKg * right);

        public static Mass operator *(UInt96 left, Mass right)
            => right * left;

        public static bool operator <=(Mass left, Mass right)
            => left.valueInKg <= right.valueInKg;

        public static bool operator >=(Mass left, Mass right)
            => left.valueInKg >= right.valueInKg;

        public static bool operator <(Mass left, Mass right)
            => left.valueInKg < right.valueInKg;

        public static bool operator >(Mass left, Mass right)
            => left.valueInKg > right.valueInKg;
    }
}
