namespace Game1.Resources
{
    [Serializable]
    public readonly record struct NumPeople : ICountable<NumPeople>
    {
        public static readonly NumPeople zero;

        static NumPeople()
            => zero = new(value: 0);

        public readonly ulong value;

        public NumPeople(ulong value)
            => this.value = value;

        public bool IsZero
            => value is 0;

        public override string ToString()
            => value.ToString();

        public static NumPeople operator +(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value + numPeople2.value);

        public static NumPeople operator -(NumPeople numPeople1, NumPeople numPeople2)
            => new(value: numPeople1.value - numPeople2.value);

        NumPeople ICountable<NumPeople>.Add(NumPeople count)
            => this + count;

        NumPeople ICountable<NumPeople>.Subtract(NumPeople count)
            => this - count;
    }
}
