namespace Game1
{
    [Serializable]
    public readonly struct ResInd : IOverlay
    {
        public const int ResCount = 3;

        private static readonly ResInd[] allInds;

        public static ResInd? MakeFrom(int resInd)
        {
            if (IsInRange(resInd: resInd))
                return new(resInd: resInd);
            return null;
        }

        static ResInd()
        {
            allInds = (from resInd in Enumerable.Range(0, ResInd.ResCount)
                       select new ResInd(resInd: resInd)).ToArray();
        }

        public static IEnumerable<ResInd> All
            => allInds;

        private readonly int resInd;

        private ResInd(int resInd)
            => this.resInd = resInd;

        public static explicit operator int(ResInd resInd)
            => resInd.resInd;

        public static explicit operator ResInd(int resInd)
            => MakeFrom(resInd: resInd).Value;
        
        public override string ToString()
            => "Res" + resInd.ToString();

        private static bool IsInRange(int resInd)
            => 0 <= resInd && resInd < ResCount;
    }
}
