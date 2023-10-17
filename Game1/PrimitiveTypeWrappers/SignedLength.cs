using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct SignedLength : IComparisonOperators<SignedLength, SignedLength, bool>,
        IAdditionOperators<SignedLength, SignedLength, SignedLength>,
        IMultiplyOperators<SignedLength, double, SignedLength>,
        ISubtractionOperators<SignedLength, SignedLength, SignedLength>,
        IDivisionOperators<SignedLength, SignedLength, double>
    {
        public static readonly SignedLength zero = new(valueInM: 0);

        public static SignedLength CreateFromM(double valueInM)
            => new(valueInM: valueInM);

        public readonly double valueInM;

        private SignedLength(double valueInM)
            => this.valueInM = valueInM;

        public Length Abs()
            => Length.CreateFromM(valueInM.Abs());

        public static explicit operator Length(SignedLength signedLength)
            => Length.CreateFromM(valueInM: (UDouble)signedLength.valueInM);

        public static SignedLength operator -(SignedLength signedLength)
            => new(-signedLength.valueInM);

        public static SignedLength operator +(SignedLength left, SignedLength right)
            => new(left.valueInM + right.valueInM);

        public static SignedLength operator -(SignedLength left, SignedLength right)
            => new(left.valueInM - right.valueInM);

        public static SignedLength operator *(SignedLength left, double right)
            => new(left.valueInM * right);

        public static SignedLength operator *(double left, SignedLength right)
            => right * left;

        public static double operator /(SignedLength left, SignedLength right)
            => left.valueInM / right.valueInM;

        public static bool operator <(SignedLength left, SignedLength right)
            => left.valueInM < right.valueInM;

        public static bool operator >(SignedLength left, SignedLength right)
            => left.valueInM > right.valueInM;

        public static bool operator <=(SignedLength left, SignedLength right)
            => left.valueInM <= right.valueInM;

        public static bool operator >=(SignedLength left, SignedLength right)
            => left.valueInM >= right.valueInM;
    }
}
