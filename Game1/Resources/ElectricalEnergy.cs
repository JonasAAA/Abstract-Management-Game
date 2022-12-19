using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct ElectricalEnergy : IUnconstrainedFormOfEnergy<ElectricalEnergy>, IComparisonOperators<ElectricalEnergy, ElectricalEnergy, bool>
    {
        public static readonly ElectricalEnergy zero;

        static ElectricalEnergy IAdditiveIdentity<ElectricalEnergy, ElectricalEnergy>.AdditiveIdentity
            => zero;

        static ElectricalEnergy()
            => zero = new(energy: Energy.zero);

        static ElectricalEnergy IUnconstrainedFormOfEnergy<ElectricalEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        public ulong ValueInJ
            => energy.valueInJ;

        public bool IsZero
            => this == zero;

        private readonly Energy energy;

        public static ElectricalEnergy CreateFromJoules(ulong valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private ElectricalEnergy(Energy energy)
            => this.energy = energy;

        public override string ToString()
            => energy.ToString();

        public static explicit operator Energy(ElectricalEnergy electricalEnergy)
            => electricalEnergy.energy;

        public static ElectricalEnergy operator +(ElectricalEnergy left, ElectricalEnergy right)
            => new(energy: left.energy + right.energy);

        public static ElectricalEnergy operator -(ElectricalEnergy left, ElectricalEnergy right)
            => new(energy: left.energy - right.energy);

        public static bool operator >(ElectricalEnergy left, ElectricalEnergy right)
            => left.energy > right.energy;

        public static bool operator >=(ElectricalEnergy left, ElectricalEnergy right)
            => left.energy >= right.energy;

        public static bool operator <(ElectricalEnergy left, ElectricalEnergy right)
            => left.energy < right.energy;

        public static bool operator <=(ElectricalEnergy left, ElectricalEnergy right)
            => left.energy <= right.energy;
    }
}
