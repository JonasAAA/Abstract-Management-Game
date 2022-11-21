using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Mass : ICountable<Mass>
    {
        public static readonly Mass zero;

        static Mass IAdditiveIdentity<Mass, Mass>.AdditiveIdentity
            => zero;

        static Mass()
            => zero = new(valueInKg: 0);

        public static Mass CreateFromKg(ulong massInKg)
            => new(valueInKg: massInKg);

        public ulong InKg
            => valueInKg;

        // This must be property rather than field so that auto-initialized mass IsZero returns true
        public bool IsZero
            => this == zero;

        private readonly ulong valueInKg;

        private Mass(ulong valueInKg)
            => this.valueInKg = valueInKg;

        public override string ToString()
            => $"{valueInKg} Kg";

        public static Mass operator +(Mass left, Mass right)
            => new(valueInKg: left.valueInKg + right.valueInKg);

        public static Mass operator -(Mass left, Mass right)
            => new(valueInKg: left.valueInKg - right.valueInKg);
    }
}
