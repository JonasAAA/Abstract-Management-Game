using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly record struct NumPeople : ICountable<NumPeople>
    {
        public static readonly NumPeople zero;

        static NumPeople IAdditiveIdentity<NumPeople, NumPeople>.AdditiveIdentity
            => zero;

        public readonly bool isZero;

        static NumPeople()
            => zero = new(value: 0);

        public readonly ulong value;

        public NumPeople(ulong value)
        {
            this.value = value;
            isZero = value == 0;
        }

        public override string ToString()
            => value.ToString();

        public static NumPeople operator +(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value + numPeople2.value);

        public static NumPeople operator -(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value - numPeople2.value);
    }
}
