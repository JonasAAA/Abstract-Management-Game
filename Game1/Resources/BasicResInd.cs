namespace Game1.Resources
{
    [Serializable]
    public readonly record struct BasicResInd
    {
        public const ulong count = 2;

        private static readonly BasicResInd[] allInds;

        public static IEnumerable<BasicResInd> All
            => allInds;

        static BasicResInd()
        {
            allInds = new BasicResInd[count];
            for (ulong ind = 0; ind < count; ind++)
                allInds[ind] = new(value: ind);
        }

        public static BasicResInd? MakeFrom(ulong value)
        {
            if (IsInRange(value: value))
                return new(value: value);
            return null;
        }

        public static BasicResInd Random()
            => new(value: C.Random(min: 0, max: count));

        private readonly ulong value;

        private BasicResInd(ulong value)
            => this.value = value;

        public override string ToString()
            => ((ResInd)this).ToString();

        private static bool IsInRange(ulong value)
            => value < count;

        public static explicit operator ulong(BasicResInd basicResInd)
            => basicResInd.value;

        public static implicit operator ResInd(BasicResInd basicResInd)
            => (ResInd)(ulong)basicResInd;

        public static explicit operator BasicResInd(ResInd resInd)
            => (BasicResInd)(ulong)resInd;

        public static explicit operator BasicResInd(ulong value)
            => MakeFrom(value: value) switch
            {
                BasicResInd basicResInd => basicResInd,
                null => throw new InvalidCastException()
            };
    }
}
