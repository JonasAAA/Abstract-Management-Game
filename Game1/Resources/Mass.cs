using System.Numerics;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Mass : IAdditionOperators<Mass, Mass, Mass>, IAdditiveIdentity<Mass, Mass>, IMultiplyOperators<Mass, ulong, Mass>, IMultiplicativeIdentity<Mass, ulong>
    {
        public static readonly Mass zero;

        static Mass IAdditiveIdentity<Mass, Mass>.AdditiveIdentity
            => zero;

        static ulong IMultiplicativeIdentity<Mass, ulong>.MultiplicativeIdentity
            => 1;

        static Mass()
            => zero = new(valueInKg: 0);

        public static Mass CreateFromKg(ulong valueInKg)
            => new(valueInKg: valueInKg);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly ulong valueInKg;

        private Mass(ulong valueInKg)
            => this.valueInKg = valueInKg;

        public override string ToString()
            => $"{valueInKg} Kg";

        public static Mass operator +(Mass left, Mass right)
            => new(valueInKg: left.valueInKg + right.valueInKg);

        public static Mass operator -(Mass left, Mass right)
            => new(valueInKg: left.valueInKg - right.valueInKg);

        public static Mass operator *(Mass left, ulong right)
            => new(valueInKg: left.valueInKg * right);

        public static Mass operator *(ulong left, Mass right)
            => right * left;
    }
}
