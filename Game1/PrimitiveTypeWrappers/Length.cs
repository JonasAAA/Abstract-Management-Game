using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct Length : IComparisonOperators<Length, Length, bool>, IComparable<Length>,
        IMin<Length>, IMax<Length>, IClose<Length>,
        IAdditionOperators<Length, Length, Length>,
        IMultiplicativeIdentity<Length, UDouble>, IMultiplyOperators<Length, UDouble, Length>,
        IMultiplyOperators<Length, Propor, Length>,
        IDivisionOperators<Length, Length, UDouble>, IDivisionOperators<Length, UDouble, Length>
    {
        static UDouble IMultiplicativeIdentity<Length, UDouble>.MultiplicativeIdentity
            => 1;

        public static readonly Length zero = new(valueInM: 0);

        public static Length CreateFromM(UDouble valueInM)
            => new(valueInM: valueInM);

        public readonly UDouble valueInM;

        private Length(UDouble valueInM)
            => this.valueInM = valueInM;

        public bool IsTiny()
            => valueInM.IsTiny();

        public bool IsCloseTo(Length other)
            => valueInM.IsCloseTo(other.valueInM);

        public int CompareTo(Length other)
            => valueInM.CompareTo(other.valueInM);

        public static implicit operator SignedLength(Length length)
            => SignedLength.CreateFromM(valueInM: length.valueInM);

        static Length IMin<Length>.Min(Length left, Length right)
            => left < right ? left : right;

        static Length IMax<Length>.Max(Length left, Length right)
            => left > right ? left : right;

        public static bool operator <(Length left, Length right)
            => left.valueInM < right.valueInM;

        public static bool operator >(Length left, Length right)
            => left.valueInM > right.valueInM;

        public static bool operator <=(Length left, Length right)
            => left.valueInM <= right.valueInM;

        public static bool operator >=(Length left, Length right)
            => left.valueInM >= right.valueInM;

        public static Length operator +(Length left, Length right)
            => new(left.valueInM + right.valueInM);

        public static Length operator *(Length left, UDouble right)
            => new(left.valueInM * right);

        public static Length operator *(UDouble left, Length right)
            => right * left;

        public static Length operator *(Length left, Propor right)
            => new(left.valueInM * right);

        public static Length operator *(Propor left, Length right)
            => right * left;

        public static UDouble operator /(Length left, Length right)
            => left.valueInM / right.valueInM;

        public static Length operator /(Length left, UDouble right)
            => new(left.valueInM / right);
    }
}
