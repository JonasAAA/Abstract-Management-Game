namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IMinable<EnergyPriority>
    {
        public static readonly EnergyPriority maximal, minimal;

        static EnergyPriority()
        {
            maximal = new(energyPriority: ulong.MaxValue);
            minimal = new(energyPriority: 0);
        }

        private readonly ulong energyPriority;

        public EnergyPriority(ulong energyPriority)
            => this.energyPriority = energyPriority;

        public static bool operator ==(EnergyPriority energyPriority1, EnergyPriority energyPriority2)
            => energyPriority1.energyPriority == energyPriority2.energyPriority;

        public static bool operator !=(EnergyPriority energyPriority1, EnergyPriority energyPriority2)
            => !(energyPriority1 == energyPriority2);

        public bool Equals(EnergyPriority other)
            => this == other;

        public override bool Equals(object obj)
            => obj is EnergyPriority energyPriority && Equals(energyPriority);

        public override int GetHashCode()
            => energyPriority.GetHashCode();

        public int CompareTo(EnergyPriority other)
            => energyPriority.CompareTo(other.energyPriority);

        public override string ToString()
            => energyPriority.ToString();

        EnergyPriority IMinable<EnergyPriority>.Min(EnergyPriority other)
            => new (energyPriority: MathHelper.Min(energyPriority, other.energyPriority));
    }
}
