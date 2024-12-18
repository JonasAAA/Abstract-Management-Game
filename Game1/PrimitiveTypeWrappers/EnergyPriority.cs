﻿using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly record struct EnergyPriority : IEquatable<EnergyPriority>, IComparable<EnergyPriority>, IComparisonOperators<EnergyPriority, EnergyPriority, bool>, IMinMaxValue<EnergyPriority>, IMax<EnergyPriority>
    {
        // These values must be here so that the EnergyPriority constructor doesn't have to reference CurWorldConfig
        // (as that would mean runtime error)
        /// <summary>
        /// 0
        /// </summary>
        public static readonly EnergyPriority leastImportant = new(value: leastImportantEnergyPrior);
        /// <summary>
        /// 100
        /// </summary>
        public static readonly EnergyPriority mostImportant = new(value: mostImportantEnergyPrior);

        static EnergyPriority IMinMaxValue<EnergyPriority>.MinValue
            => leastImportant;

        static EnergyPriority IMinMaxValue<EnergyPriority>.MaxValue
            => mostImportant;

        private const ulong leastImportantEnergyPrior = 0, mostImportantEnergyPrior = 100;

        private readonly ulong value;

        /// <summary>
        /// The higher, the more important.
        /// 0 - least important, 100 - most important
        /// </summary>
        public EnergyPriority(ulong value)
        {
            if (value is < leastImportantEnergyPrior or > mostImportantEnergyPrior)
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
