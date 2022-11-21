using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IComparisonOperators<EnergyPriority, EnergyPriority, bool>, IPrimitiveTypeWrapper
    {
        public static readonly EnergyPriority maximal, minimal;

        static EnergyPriority()
        {
            maximal = new(value: ulong.MaxValue);
            minimal = new(value: 0);
        }

        private readonly ulong value;

        public EnergyPriority(ulong value)
            => this.value = value;

        public int CompareTo(EnergyPriority other)
            => value.CompareTo(other.value);

        public string ToString(string? format, IFormatProvider? formatProvider)
            => $"energy priority {value.ToString(format, formatProvider)}";

        public static bool operator >(EnergyPriority left, EnergyPriority right)
            => left.value > right.value;

        public static bool operator >=(EnergyPriority left, EnergyPriority right)
            => left.value >= right.value;

        public static bool operator <(EnergyPriority left, EnergyPriority right)
            => left.value < right.value;

        public static bool operator <=(EnergyPriority left, EnergyPriority right)
            => left.value <= right.value;
    }
}
