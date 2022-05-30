namespace Game1.Resources
{
    [Serializable]
    public readonly record struct ResInd : IOverlay
    {
        public const ulong count = BasicResInd.count + NonBasicResInd.count;

        private static readonly ResInd[] allInds;

        public static IEnumerable<ResInd> All
            => allInds;

        static ResInd()
        {
            allInds = new ResInd[count];
            for (ulong ind = 0; ind < count; ind++)
                allInds[ind] = new(value: ind);
        }

        public static ResInd? MakeFrom(ulong value)
        {
            if (IsInRange(value: value))
                return new(value: value);
            return null;
        }

        private readonly ulong value;

        private ResInd(ulong value)
            => this.value = value;

        public static explicit operator ulong(ResInd resInd)
            => resInd.value;

        public static explicit operator int(ResInd resInd)
            => (int)(ulong)resInd;

        public static explicit operator ResInd(ulong value)
            => MakeFrom(value: value) switch
            {
                ResInd resInd => resInd,
                null => throw new InvalidCastException()
            };

        public static bool operator <(ResInd resInd1, ResInd resInd2)
            => resInd1.value < resInd2.value;

        public static bool operator >(ResInd resInd1, ResInd resInd2)
            => resInd1.value > resInd2.value;

        public static bool operator <=(ResInd resInd1, ResInd resInd2)
            => resInd1.value <= resInd2.value;

        public static bool operator >=(ResInd resInd1, ResInd resInd2)
            => resInd1.value >= resInd2.value;

        public override string ToString()
            => "Res" + value.ToString();

        private static bool IsInRange(ulong value)
            => value < count;
    }
}
