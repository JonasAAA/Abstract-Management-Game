namespace Game1.Resources
{
    [Serializable]
    public readonly struct Mass
    {
        public static readonly Mass zero;

        static Mass()
            => zero = new Mass(valueInKg: 0);

        public static Mass CreateFromKg(ulong massInKg)
            => new(valueInKg: massInKg);

        public ulong InKg
            => valueInKg;

        private readonly ulong valueInKg;

        private Mass(ulong valueInKg)
            => this.valueInKg = valueInKg;

        public bool IsZero
            => valueInKg is 0;

        public override string ToString()
            => $"{valueInKg} Kg";

        public static Mass operator +(Mass mass1, Mass mass2)
            => new(valueInKg: mass1.valueInKg + mass2.valueInKg);

        public static Mass operator -(Mass mass1, Mass mass2)
            => new(valueInKg: mass1.valueInKg - mass2.valueInKg);
    }
}
