using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Area : IOrderedVector<Area, UInt96>, IMin<Area>, IMax<Area>
    {
        public static readonly Area zero = new(valueInMetSq: 0);

        static Area IAdditiveIdentity<Area, Area>.AdditiveIdentity
            => zero;

        static UInt96 IMultiplicativeIdentity<Area, UInt96>.MultiplicativeIdentity
            => 1;

        public static Area CreateFromMetSq(UInt96 valueInMetSq)
            => new(valueInMetSq: valueInMetSq);

        // This must be property rather than field so that auto-initialized Area IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UInt96 valueInMetSq;

        private Area(UInt96 valueInMetSq)
            => this.valueInMetSq = valueInMetSq;

        public static Area Min(Area left, Area right)
            => left.valueInMetSq < right.valueInMetSq ? left : right;

        public static Area Max(Area left, Area right)
            => left.valueInMetSq > right.valueInMetSq ? left : right;

        public override string ToString()
            => $"{valueInMetSq} m^2";

        public static Area operator +(Area left, Area right)
            => new(valueInMetSq: left.valueInMetSq + right.valueInMetSq);

        public static Area operator -(Area left, Area right)
            => new(valueInMetSq: left.valueInMetSq - right.valueInMetSq);

        public static Area operator *(Area left, UInt96 right)
            => new(valueInMetSq: left.valueInMetSq * right);

        public static Area operator *(UInt96 left, Area right)
            => right * left;

        public static bool operator <=(Area left, Area right)
            => left.valueInMetSq <= right.valueInMetSq;

        public static bool operator >=(Area left, Area right)
            => left.valueInMetSq >= right.valueInMetSq;

        public static bool operator <(Area left, Area right)
            => left.valueInMetSq < right.valueInMetSq;

        public static bool operator >(Area left, Area right)
            => left.valueInMetSq > right.valueInMetSq;
    }
}
