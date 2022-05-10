namespace Game1.Resources
{
    [Serializable]
    public readonly record struct NonBasicResInd
    {
        public const ulong count = 3;

        private static readonly NonBasicResInd[] allInds;

        public static IEnumerable<NonBasicResInd> All
            => allInds;

        static NonBasicResInd()
        {
            allInds = new NonBasicResInd[count];
            for (ulong ind = 0; ind < count; ind++)
                allInds[ind] = new(value: BasicResInd.count + ind);
        }

        public static NonBasicResInd? MakeFrom(ulong value)
        {
            if (IsInRange(value: value))
                return new(value: value);
            return null;
        }

        public static NonBasicResInd Random()
            => new(value: BasicResInd.count + C.Random(min: 0, max: count));

        private readonly ulong value;

        private NonBasicResInd(ulong value)
            => this.value = value;

        public override string ToString()
            => ((ResInd)this).ToString();

        private static bool IsInRange(ulong value)
            => value - BasicResInd.count is >= 0 and < count;

        public static explicit operator ulong(NonBasicResInd nonBasicResInd)
            => nonBasicResInd.value;

        public static implicit operator ResInd(NonBasicResInd nonBasicResInd)
            => (ResInd)(ulong)nonBasicResInd;

        public static explicit operator NonBasicResInd(ResInd resInd)
            => (NonBasicResInd)(ulong)resInd;

        public static explicit operator NonBasicResInd(ulong value)
            => MakeFrom(value: value) switch
            {
                NonBasicResInd nonBasicResInd => nonBasicResInd,
                null => throw new InvalidCastException()
            };
    }
}
