using System.Numerics;

namespace Game1.Resources
{
    /// <summary>
    /// The amount of energy needed to increase the temperature by one degree
    /// </summary>
    [Serializable]
    public readonly record struct HeatCapacity : IOrderedVector<HeatCapacity, ulong>
    {
        public static readonly HeatCapacity zero = new(valueInJPerK: 0);

        static HeatCapacity IAdditiveIdentity<HeatCapacity, HeatCapacity>.AdditiveIdentity
            => zero;

        static ulong IMultiplicativeIdentity<HeatCapacity, ulong>.MultiplicativeIdentity
            => 1;

        public static HeatCapacity CreateFromJPerK(ulong valueInJPerK)
            => new(valueInJPerK: valueInJPerK);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        // value in joules per kelvin
        public readonly ulong valueInJPerK;

        private HeatCapacity(ulong valueInJPerK)
            => this.valueInJPerK = valueInJPerK;

        public override string ToString()
            => $"{valueInJPerK} J/K";

        //static HeatCapacity IMin<HeatCapacity>.Min(HeatCapacity left, HeatCapacity right)
        //    => left < right ? left : right;

        public static HeatCapacity operator +(HeatCapacity left, HeatCapacity right)
            => new(valueInJPerK: left.valueInJPerK + right.valueInJPerK);

        public static HeatCapacity operator -(HeatCapacity left, HeatCapacity right)
            => new(valueInJPerK: left.valueInJPerK - right.valueInJPerK);

        public static HeatCapacity operator *(HeatCapacity left, ulong right)
            => new(valueInJPerK: left.valueInJPerK * right);

        public static HeatCapacity operator *(ulong left, HeatCapacity right)
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
