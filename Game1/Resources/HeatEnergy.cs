using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct HeatEnergy : IUnconstrainedEnergy<HeatEnergy> //IMultiplyOperators<HeatEnergy, UDouble, HeatEnergy>
    {
        public static readonly HeatEnergy zero = new(energy: Energy.zero);

        static HeatEnergy IAdditiveIdentity<HeatEnergy, HeatEnergy>.AdditiveIdentity
            => zero;

        static HeatEnergy IUnconstrainedEnergy<HeatEnergy>.CreateFromEnergy(Energy energy)
            => new(energy: energy);

        public bool IsZero
            => this == zero;

        public UInt96 ValueInJ
            => energy.valueInJ;

        private readonly Energy energy;

        public static HeatEnergy CreateFromJoules(UInt96 valueInJ)
            => new(energy: Energy.CreateFromJoules(valueInJ: valueInJ));

        private HeatEnergy(Energy energy)
            => this.energy = energy;

        public override string ToString()
            => energy.ToString();

        static HeatEnergy IMin<HeatEnergy>.Min(HeatEnergy left, HeatEnergy right)
            => MyMathHelper.TotalOrderMin(left, right);

        public static explicit operator Energy(HeatEnergy heatEnergy)
            => heatEnergy.energy;

        public static HeatEnergy operator +(HeatEnergy left, HeatEnergy right)
            => new(energy: left.energy + right.energy);

        public static HeatEnergy operator -(HeatEnergy left, HeatEnergy right)
            => new(energy: left.energy - right.energy);

        public static bool operator >(HeatEnergy left, HeatEnergy right)
            => left.ValueInJ > right.ValueInJ;

        public static bool operator >=(HeatEnergy left, HeatEnergy right)
            => left.ValueInJ >= right.ValueInJ;

        public static bool operator <(HeatEnergy left, HeatEnergy right)
            => left.ValueInJ < right.ValueInJ;

        public static bool operator <=(HeatEnergy left, HeatEnergy right)
            => left.ValueInJ <= right.ValueInJ;

        //public static HeatEnergy operator *(UInt96 left, HeatEnergy right)
        //    => new(energy: left * right.energy);

        //public static HeatEnergy operator *(HeatEnergy left, UInt96 right)
        //    => right * left;
    }
}
