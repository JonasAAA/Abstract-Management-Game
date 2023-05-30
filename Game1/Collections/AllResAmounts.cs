using System.Numerics;

namespace Game1.Collections
{
    [Serializable]
    public readonly record struct AllResAmounts : IResAmounts<AllResAmounts>
    {
        public static readonly AllResAmounts empty;

        static AllResAmounts IAdditiveIdentity<AllResAmounts, AllResAmounts>.AdditiveIdentity
            => empty;

        static ulong IMultiplicativeIdentity<AllResAmounts, ulong>.MultiplicativeIdentity
            => 1;

        static AllResAmounts()
        {
            empty = new(SomeResAmounts<IResource>.empty, RawMaterialsMix.empty);
        }

        public bool IsEmpty
            => resAmounts.IsEmpty && rawMatsMix.IsEmpty;

        bool IFormOfEnergy<AllResAmounts>.IsZero
            => IsEmpty;

        public static AllResAmounts CreateFromNoMix(SomeResAmounts<IResource> resAmounts)
            => new(resAmounts: resAmounts, rawMatsMix: RawMaterialsMix.empty);

        public static AllResAmounts CreateFromOnlyMix(RawMaterialsMix rawMatsMix)
            => new(resAmounts: SomeResAmounts<IResource>.empty, rawMatsMix: rawMatsMix);

        public readonly SomeResAmounts<IResource> resAmounts;
        public readonly RawMaterialsMix rawMatsMix;

        private AllResAmounts(SomeResAmounts<IResource> resAmounts, RawMaterialsMix rawMatsMix)
        {
            this.resAmounts = resAmounts;
            this.rawMatsMix = rawMatsMix;
        }

        public Mass Mass()
            => resAmounts.Mass() + rawMatsMix.Mass();

        public HeatCapacity HeatCapacity()
            => resAmounts.HeatCapacity() + rawMatsMix.HeatCapacity();

        //public Area Area()
        //    => resAmounts.Area() + rawMatsMix.Area();

        public RawMaterialsMix RawMatComposition()
            => resAmounts.RawMatComposition() + rawMatsMix.RawMatComposition();

        public static AllResAmounts operator +(AllResAmounts left, AllResAmounts right)
            => new(left.resAmounts + right.resAmounts, left.rawMatsMix + right.rawMatsMix);

        public static AllResAmounts operator -(AllResAmounts left, AllResAmounts right)
            => new(left.resAmounts - right.resAmounts, left.rawMatsMix - right.rawMatsMix);

        public static AllResAmounts operator *(AllResAmounts left, ulong right)
            => new(left.resAmounts * right, left.rawMatsMix * right);

        public static AllResAmounts operator *(ulong left, AllResAmounts right)
            => right * left;

        public static explicit operator Energy(AllResAmounts formOfEnergy)
            => (Energy)formOfEnergy.resAmounts + (Energy)formOfEnergy.rawMatsMix;

        public static bool operator >=(AllResAmounts left, AllResAmounts right)
            => left.resAmounts >= right.resAmounts && left.rawMatsMix >= right.rawMatsMix;

        public static bool operator <=(AllResAmounts left, AllResAmounts right)
            => right >= left;

        static bool IComparisonOperators<AllResAmounts, AllResAmounts, bool>.operator <(AllResAmounts left, AllResAmounts right)
            => left <= right && left != right;

        static bool IComparisonOperators<AllResAmounts, AllResAmounts, bool>.operator >(AllResAmounts left, AllResAmounts right)
            => left >= right && left != right;

        static AllResAmounts IMin<AllResAmounts>.Min(AllResAmounts left, AllResAmounts right)
            => new
            (
                MyMathHelper.Min(left.resAmounts, right.resAmounts),
                MyMathHelper.Min(left.rawMatsMix, right.rawMatsMix)
            );
    }
}
