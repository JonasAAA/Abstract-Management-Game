namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IMinable<EnergyPriority>, IPrimitiveTypeWrapper
    {
        public static readonly EnergyPriority maximal, minimal;

        static EnergyPriority()
        {
            maximal = new(value: ulong.MaxValue);
            minimal = new(value: 0);
        }

        private readonly ulong value;

        public EnergyPriority(ulong value)
            => this.value = value;

        public static bool operator ==(EnergyPriority energyPriority1, EnergyPriority energyPriority2)
            => energyPriority1.value == energyPriority2.value;

        public static bool operator !=(EnergyPriority energyPriority1, EnergyPriority energyPriority2)
            => !(energyPriority1 == energyPriority2);

        public bool Equals(EnergyPriority other)
            => this == other;

        public override bool Equals(object obj)
            => obj is EnergyPriority energyPriority && Equals(energyPriority);

        public override int GetHashCode()
            => value.GetHashCode();

        public int CompareTo(EnergyPriority other)
            => value.CompareTo(other.value);

        EnergyPriority IMinable<EnergyPriority>.Min(EnergyPriority other)
            => new(value: MyMathHelper.Min(value, other.value));

        public string ToString(string format, IFormatProvider formatProvider)
            =>$"energy priority {value.ToString(format, formatProvider)}";
    }
}
