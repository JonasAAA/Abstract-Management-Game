using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct RadiantEnergy : IUnconstrainedEnergy<RadiantEnergy>
    {
        public static readonly RadiantEnergy zero = new(energy: Energy.zero);

        static RadiantEnergy IAdditiveIdentity<RadiantEnergy, RadiantEnergy>.AdditiveIdentity
            => zero;

        static RadiantEnergy IUnconstrainedEnergy<RadiantEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        public bool IsZero
            => this == zero;

        public UInt96 ValueInJ
            => energy.valueInJ;

        private readonly Energy energy;

        public static RadiantEnergy CreateFromJoules(UInt96 valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private RadiantEnergy(Energy energy)
            => this.energy = energy;

        public RadiantEnergy TakeApproxPropor(Propor propor)
            => new(energy: Energy.CreateFromJoules(valueInJ: Convert.ToUInt64(ValueInJ * (decimal)propor)));

        public override string ToString()
            => energy.ToString();

        static RadiantEnergy IMin<RadiantEnergy>.Min(RadiantEnergy left, RadiantEnergy right)
            => MyMathHelper.TotalOrderMin(left, right);

        public static explicit operator Energy(RadiantEnergy radiantEnergy)
            => radiantEnergy.energy;

        public static RadiantEnergy operator +(RadiantEnergy left, RadiantEnergy right)
            => new(energy: left.energy + right.energy);

        public static RadiantEnergy operator -(RadiantEnergy left, RadiantEnergy right)
            => new(energy: left.energy - right.energy);

        public static bool operator >(RadiantEnergy left, RadiantEnergy right)
            => left.ValueInJ > right.ValueInJ;

        public static bool operator >=(RadiantEnergy left, RadiantEnergy right)
            => left.ValueInJ >= right.ValueInJ;

        public static bool operator <(RadiantEnergy left, RadiantEnergy right)
            => left.ValueInJ < right.ValueInJ;

        public static bool operator <=(RadiantEnergy left, RadiantEnergy right)
            => left.ValueInJ <= right.ValueInJ;
    }
}
