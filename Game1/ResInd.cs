namespace Game1
{
    [Serializable]
    public readonly struct ResInd
    {
        public const int ResCount = (int)WorldManager.MaxRes + 1;

        private static readonly ResInd[] allInds;

        public static ResInd? MakeFrom(int resInd)
        {
            if (IsInRange(resInd: resInd))
                return new(resInd: resInd);
            return null;
        }

        public static ResInd? MakeFrom(Overlay overlay)
            => MakeFrom((int)overlay);

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
        
        public static explicit operator Overlay(ResInd resInd)
            => (Overlay)(int)resInd;

        public static explicit operator ResInd(Overlay overlay)
            => (ResInd)(int)overlay;

        public override string ToString()
            => resInd.ToString();

        private static bool IsInRange(int resInd)
            => 0 <= resInd && resInd < ResCount;
    }
}
