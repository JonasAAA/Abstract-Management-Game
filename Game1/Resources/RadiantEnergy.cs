using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct RadiantEnergy : IUnconstrainedFormOfEnergy<RadiantEnergy>
    {
        private static readonly RadiantEnergy zero;

        static RadiantEnergy IAdditiveIdentity<RadiantEnergy, RadiantEnergy>.AdditiveIdentity
            => zero;

        static RadiantEnergy()
            => zero = new(energy: Energy.zero);

        public ulong ValueInJ
            => energy.valueInJ;

        static RadiantEnergy IUnconstrainedFormOfEnergy<RadiantEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        Energy IFormOfEnergy<RadiantEnergy>.Energy
            => energy;

        private readonly Energy energy;

        public static RadiantEnergy CreateFromJoules(ulong valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private RadiantEnergy(Energy energy)
            => this.energy = energy;

        public override string ToString()
            => energy.ToString();

        public static RadiantEnergy operator +(RadiantEnergy left, RadiantEnergy right)
            => new(energy: left.energy + right.energy);

        public static RadiantEnergy operator -(RadiantEnergy left, RadiantEnergy right)
            => new(energy: left.energy - right.energy);
    }
}
