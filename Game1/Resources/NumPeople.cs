using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct NumPeople : ICountable<NumPeople>
    {
        public static readonly NumPeople zero;

        static NumPeople IAdditiveIdentity<NumPeople, NumPeople>.AdditiveIdentity
            => zero;

        static NumPeople()
            => zero = new(value: 0);

        // This must be property rather than field so that auto-initialized numPeople IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly ulong value;

        public NumPeople(ulong value)
            => this.value = value;

        public override string ToString()
            => value.ToString();

        static NumPeople IMin<NumPeople>.Min(NumPeople left, NumPeople right)
            => left < right ? left : right;

        public static NumPeople operator +(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value + numPeople2.value);

        public static NumPeople operator -(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value - numPeople2.value);

        public static bool operator >(NumPeople left, NumPeople right)
            => left.value > right.value;

        public static bool operator >=(NumPeople left, NumPeople right)
            => left.value >= right.value;

        public static bool operator <(NumPeople left, NumPeople right)
            => left.value < right.value;

        public static bool operator <=(NumPeople left, NumPeople right)
            => left.value <= right.value;
    }
}
