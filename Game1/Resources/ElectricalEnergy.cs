using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct ElectricalEnergy : IUnconstrainedEnergy<ElectricalEnergy>, IComparisonOperators<ElectricalEnergy, ElectricalEnergy, bool>
    {
        public static readonly ElectricalEnergy zero = new(energy: Energy.zero);

        static ElectricalEnergy IAdditiveIdentity<ElectricalEnergy, ElectricalEnergy>.AdditiveIdentity
            => zero;

        static ElectricalEnergy IUnconstrainedEnergy<ElectricalEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        public UInt96 ValueInJ
            => energy.valueInJ;

        public bool IsZero
            => this == zero;

        private readonly Energy energy;

        public static ElectricalEnergy CreateFromJoules(UInt96 valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private ElectricalEnergy(Energy energy)
            => this.energy = energy;

        public override string ToString()
            => energy.ToString();

        static ElectricalEnergy IMin<ElectricalEnergy>.Min(ElectricalEnergy left, ElectricalEnergy right)
            => MyMathHelper.TotalOrderMin(left, right);

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
