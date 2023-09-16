using System.Numerics;

namespace Game1.Resources
{
    /// <summary>
    /// The amount of energy needed to increase the temperature by one degree
    /// </summary>
    [Serializable]
    public readonly record struct HeatCapacity : IOrderedVector<HeatCapacity, UInt96>
    {
        public static readonly HeatCapacity zero = new(valueInJPerK: 0);

        static HeatCapacity IAdditiveIdentity<HeatCapacity, HeatCapacity>.AdditiveIdentity
            => zero;

        static UInt96 IMultiplicativeIdentity<HeatCapacity, UInt96>.MultiplicativeIdentity
            => 1;

        public static HeatCapacity CreateFromJPerK(UInt96 valueInJPerK)
            => new(valueInJPerK: valueInJPerK);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        // value in joules per kelvin
        public readonly UInt96 valueInJPerK;

        private HeatCapacity(UInt96 valueInJPerK)
            => this.valueInJPerK = valueInJPerK;

        public override string ToString()
            => $"{valueInJPerK} J/K";

        public static HeatCapacity operator +(HeatCapacity left, HeatCapacity right)
            => new(valueInJPerK: left.valueInJPerK + right.valueInJPerK);

        public static HeatCapacity operator -(HeatCapacity left, HeatCapacity right)
            => new(valueInJPerK: left.valueInJPerK - right.valueInJPerK);

        public static HeatCapacity operator *(HeatCapacity left, UInt96 right)
            => new(valueInJPerK: left.valueInJPerK * right);

        public static HeatCapacity operator *(UInt96 left, HeatCapacity right)
            => right * left;

        public static bool operator >=(HeatCapacity left, HeatCapacity right)
            => left.valueInJPerK >= right.valueInJPerK;

        public static bool operator <=(HeatCapacity left, HeatCapacity right)
            => left.valueInJPerK <= right.valueInJPerK;

        public static bool operator >(HeatCapacity left, HeatCapacity right)
            => left.valueInJPerK > right.valueInJPerK;

        public static bool operator <(HeatCapacity left, HeatCapacity right)
            => left.valueInJPerK < right.valueInJPerK;
    }
}
