using System.Numerics;

namespace Game1.Resources
{
    public readonly record struct Energy //: ICountable<Energy> //, IMultiplyOperators<Energy, ulong, Energy>
    {
        public static readonly Energy zero;

        //static Energy IAdditiveIdentity<Energy, Energy>.AdditiveIdentity
        //    => zero;

        static Energy()
            => zero = new(valueInJ: 0);

        public static Energy CreateFromJoules(ulong valueInJ)
            => new(valueInJ: valueInJ);

        // This must be property rather than field so that auto-initialized Mass IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly ulong valueInJ;

        private Energy(ulong valueInJ)
            => this.valueInJ = valueInJ;

        public override string ToString()
            => $"{valueInJ} J";

        public static Energy operator +(Energy left, Energy right)
            => new(valueInJ: left.valueInJ + right.valueInJ);

        public static Energy operator -(Energy left, Energy right)
            => new(valueInJ: left.valueInJ - right.valueInJ);

        //public static Energy operator *(ulong left, Energy right)
        //    => new(valueInJ: left * right.valueInJ);

        //public static Energy operator *(Energy left, ulong right)
        //    => right * left;
    }
}
