using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Energy : IAdditionOperators<Energy, Energy, Energy>, IAdditiveIdentity<Energy, Energy>, IComparisonOperators<Energy, Energy, bool>
    {
        public static readonly Energy zero = new(valueInJ: 0);

        static Energy IAdditiveIdentity<Energy, Energy>.AdditiveIdentity
            => zero;

        public static Energy CreateFromJoules(UInt96 valueInJ)
            => new(valueInJ: valueInJ);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UInt96 valueInJ;

        private Energy(UInt96 valueInJ)
            => this.valueInJ = valueInJ;

        public override string ToString()
            => $"{valueInJ} J";

        public static Energy operator +(Energy left, Energy right)
            => new(valueInJ: left.valueInJ + right.valueInJ);

        public static Energy operator -(Energy left, Energy right)
            => new(valueInJ: left.valueInJ - right.valueInJ);

        public static bool operator >(Energy left, Energy right)
            => left.valueInJ > right.valueInJ;

        public static bool operator >=(Energy left, Energy right)
            => left.valueInJ >= right.valueInJ;

        public static bool operator <(Energy left, Energy right)
            => left.valueInJ < right.valueInJ;

        public static bool operator <=(Energy left, Energy right)
            => left.valueInJ <= right.valueInJ;

        //public static Energy operator *(UInt96 left, Energy right)
        //    => new(valueInJ: left * right.valueInJ);

        //public static Energy operator *(Energy left, UInt96 right)
        //    => right * left;
    }
}
