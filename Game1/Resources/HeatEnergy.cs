using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct HeatEnergy : IUnconstrainedFormOfEnergy<HeatEnergy> //IMultiplyOperators<HeatEnergy, UDouble, HeatEnergy>
    {
        private static readonly HeatEnergy zero;

        static HeatEnergy IAdditiveIdentity<HeatEnergy, HeatEnergy>.AdditiveIdentity
            => zero;

        static HeatEnergy()
            => zero = new(energy: Energy.zero);

        public ulong ValueInJ
            => energy.valueInJ;

        static HeatEnergy IUnconstrainedFormOfEnergy<HeatEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        private readonly Energy energy;

        public static HeatEnergy CreateFromJoules(ulong valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private HeatEnergy(Energy energy)
            => this.energy = energy;

        public override string ToString()
            => energy.ToString();

        public static explicit operator Energy(HeatEnergy heatEnergy)
            => heatEnergy.energy;

        public static HeatEnergy operator +(HeatEnergy left, HeatEnergy right)
            => new(energy: left.energy + right.energy);

        public static HeatEnergy operator -(HeatEnergy left, HeatEnergy right)
            => new(energy: left.energy - right.energy);

        //public static HeatEnergy operator *(ulong left, HeatEnergy right)
        //    => new(energy: left * right.energy);

        //public static HeatEnergy operator *(HeatEnergy left, ulong right)
        //    => right * left;
    }
}
