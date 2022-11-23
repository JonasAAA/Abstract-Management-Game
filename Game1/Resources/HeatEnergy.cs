using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct HeatEnergy : ICountable<HeatEnergy>, IMultiplyOperators<HeatEnergy, UDouble, HeatEnergy>
    {
        private static readonly HeatEnergy zero;

        static HeatEnergy IAdditiveIdentity<HeatEnergy, HeatEnergy>.AdditiveIdentity
            => zero;

        static HeatEnergy()
            => zero = new(valueInJoules: 0);

        private readonly UDouble valueInJoules;

        public static HeatEnergy CreateFromJoules(UDouble valueInJoules)
            => new(valueInJoules: valueInJoules);

        private HeatEnergy(UDouble valueInJoules)
            => this.valueInJoules = valueInJoules;

        public override string ToString()
            => $"{valueInJoules} J";

        public static HeatEnergy operator +(HeatEnergy left, HeatEnergy right)
            => new(valueInJoules: left.valueInJoules + right.valueInJoules);

        public static HeatEnergy operator -(HeatEnergy left, HeatEnergy right)
            => new(valueInJoules: (UDouble)(left.valueInJoules - right.valueInJoules));

        public static HeatEnergy operator *(UDouble left, HeatEnergy right)
            => new(valueInJoules: left * right.valueInJoules);

        public static HeatEnergy operator *(HeatEnergy left, UDouble right)
            => right * left;
    }
}
