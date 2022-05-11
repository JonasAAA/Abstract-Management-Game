namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IMinable<EnergyPriority>, IPrimitiveTypeWrapper
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

        public int CompareTo(EnergyPriority other)
            => value.CompareTo(other.value);

        EnergyPriority IMinable<EnergyPriority>.Min(EnergyPriority other)
            => new(value: MyMathHelper.Min(value, other.value));

        public string ToString(string? format, IFormatProvider? formatProvider)
            => $"energy priority {value.ToString(format, formatProvider)}";
    }
}
