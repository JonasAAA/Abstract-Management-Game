using System.Numerics;
using static Game1.WorldManager;

namespace Game1.PrimitiveTypeWrappers
{
    /// <summary>
    /// The higher, the more important
    /// 0 - least important, 100 - most important
    /// </summary>
    [Serializable]
    public readonly record struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IPrimitiveTypeWrapper, IComparisonOperators<EnergyPriority, EnergyPriority, bool>, IMinMaxValue<EnergyPriority>, IMax<EnergyPriority>
    {
        /// <summary>
        /// I.e. ulong.MaxValue
        /// </summary>
        public static readonly EnergyPriority leastImportant;
        /// <summary>
        /// I.e. 0
        /// </summary>
        public static readonly EnergyPriority mostImportant;

        static EnergyPriority IMinMaxValue<EnergyPriority>.MinValue
            => leastImportant;

        static EnergyPriority IMinMaxValue<EnergyPriority>.MaxValue
            => mostImportant;

        private const ulong leastImportantEnergyPrior = 0, mostImportantEnergyPrior = 100;

        static EnergyPriority()
        {
            // These values must be here so that the EnergyPriority constructor doesn't have to reference CurWorldConfig
            // (as that would mean runtime error)
            leastImportant = new(value: leastImportantEnergyPrior);
            mostImportant = new(value: mostImportantEnergyPrior);
        }

        private readonly ulong value;

        public EnergyPriority(ulong value)
        {
            if (value < leastImportantEnergyPrior || value > mostImportantEnergyPrior)
                throw new ArgumentException();
            this.value = value;
        }

        public int CompareTo(EnergyPriority other)
            => value.CompareTo(other.value);

        public string ToString(string? format, IFormatProvider? formatProvider)
            => $"energy priority {value.ToString(format, formatProvider)}";

        static EnergyPriority IMax<EnergyPriority>.Max(EnergyPriority left, EnergyPriority right)
            => left > right ? left : right;

        public static bool operator >(EnergyPriority left, EnergyPriority right)
            => left.value > right.value;

        public static bool operator >=(EnergyPriority left, EnergyPriority right)
            => left.value >= right.value;

        public static bool operator <(EnergyPriority left, EnergyPriority right)
            => left.value < right.value;

        public static bool operator <=(EnergyPriority left, EnergyPriority right)
            => left.value <= right.value;
    }
}
