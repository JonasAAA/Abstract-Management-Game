using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct Area<T> : IOrderedVector<Area<T>, T>, IMin<Area<T>>, IMax<Area<T>>
        where T : struct, IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplyOperators<T, T, T>, IMultiplicativeIdentity<T, T>,
            IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T>
    {
        public static readonly Area<T> zero;

        static Area<T> IAdditiveIdentity<Area<T>, Area<T>>.AdditiveIdentity
            => zero;

        static T IMultiplicativeIdentity<Area<T>, T>.MultiplicativeIdentity
            => T.MultiplicativeIdentity;

        static Area()
            => zero = new(valueInMetSq: T.AdditiveIdentity);

        public static Area<T> CreateFromMetSq(T valueInMetSq)
            => new(valueInMetSq: valueInMetSq);

        // This must be property rather than field so that auto-initialized Area IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly T valueInMetSq;

        private Area(T valueInMetSq)
            => this.valueInMetSq = valueInMetSq;

        public static Area<T> Min(Area<T> left, Area<T> right)
            => left.valueInMetSq < right.valueInMetSq ? left : right;

        public static Area<T> Max(Area<T> left, Area<T> right)
            => left.valueInMetSq > right.valueInMetSq ? left : right;

        public override string ToString()
            => $"{valueInMetSq} m^2";

        public static Area<T> operator +(Area<T> left, Area<T> right)
            => new(valueInMetSq: left.valueInMetSq + right.valueInMetSq);

        public static Area<T> operator -(Area<T> left, Area<T> right)
            => new(valueInMetSq: left.valueInMetSq - right.valueInMetSq);

        public static Area<T> operator *(Area<T> left, T right)
            => new(valueInMetSq: left.valueInMetSq * right);

        public static Area<T> operator *(T left, Area<T> right)
            => right * left;

        public static bool operator <=(Area<T> left, Area<T> right)
            => left.valueInMetSq <= right.valueInMetSq;

        public static bool operator >=(Area<T> left, Area<T> right)
            => left.valueInMetSq >= right.valueInMetSq;

        public static bool operator <(Area<T> left, Area<T> right)
            => left.valueInMetSq < right.valueInMetSq;

        public static bool operator >(Area<T> left, Area<T> right)
            => left.valueInMetSq > right.valueInMetSq;
    }
}
