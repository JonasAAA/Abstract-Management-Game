using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct NumPeople : ICountable<NumPeople>
    {
        public static readonly NumPeople zero = new(value: 0);

        static NumPeople IAdditiveIdentity<NumPeople, NumPeople>.AdditiveIdentity
            => zero;

        // This must be property rather than field so that auto-initialized numPeople IsZero returns true
        public bool IsZero
            => this == zero;

        public readonly UInt96 value;

        public NumPeople(UInt96 value)
            => this.value = value;

        public override string ToString()
            => value.ToString();

        static NumPeople IMin<NumPeople>.Min(NumPeople left, NumPeople right)
            => MyMathHelper.TotalOrderMin(left, right);

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
