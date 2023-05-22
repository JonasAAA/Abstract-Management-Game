using System.Numerics;

namespace Game1.Collections
{
    [Serializable]
    public readonly struct AllResAmounts : IResAmounts<AllResAmounts>
    {
        public static readonly AllResAmounts empty;

        static AllResAmounts IAdditiveIdentity<AllResAmounts, AllResAmounts>.AdditiveIdentity
            => empty;

        static ulong IMultiplicativeIdentity<AllResAmounts, ulong>.MultiplicativeIdentity
            => 1;

        static AllResAmounts()
        {
            empty = new(SomeResAmounts<IResource>.empty, SomeResAmounts<RawMaterial>.empty);
        }

        public bool IsEmpty
            => resAmounts.IsEmpty && rawMatsMix.IsEmpty;

        bool IFormOfEnergy<AllResAmounts>.IsZero
            => IsEmpty;

        public static AllResAmounts CreateFromNoMix(SomeResAmounts<IResource> resAmounts)
            => new(resAmounts: resAmounts, rawMatsMix: SomeResAmounts<RawMaterial>.empty);

        public static AllResAmounts CreateFromOnlyMix(SomeResAmounts<RawMaterial> rawMatsMix)
            => new(resAmounts: SomeResAmounts<IResource>.empty, rawMatsMix: rawMatsMix);

        public readonly SomeResAmounts<IResource> resAmounts;
        public readonly SomeResAmounts<RawMaterial> rawMatsMix;

        private AllResAmounts(SomeResAmounts<IResource> resAmounts, SomeResAmounts<RawMaterial> rawMatsMix)
        {
            this.resAmounts = resAmounts;
            this.rawMatsMix = rawMatsMix;
        }

        public Mass Mass()
            => resAmounts.Mass() + rawMatsMix.Mass();

        public HeatCapacity HeatCapacity()
            => resAmounts.HeatCapacity() + rawMatsMix.HeatCapacity();

        public Area Area()
            => resAmounts.Area() + rawMatsMix.Area();

        public static AllResAmounts operator +(AllResAmounts left, AllResAmounts right)
            => new(left.resAmounts + right.resAmounts, left.rawMatsMix + right.rawMatsMix);

        public static AllResAmounts operator -(AllResAmounts left, AllResAmounts right)
            => new(left.resAmounts - right.resAmounts, left.rawMatsMix - right.rawMatsMix);

        public static AllResAmounts operator *(AllResAmounts left, ulong right)
            => new(left.resAmounts * right, left.rawMatsMix * right);

        public static AllResAmounts operator *(ulong left, AllResAmounts right)
            => right * left;

        public static bool operator ==(AllResAmounts left, AllResAmounts right)
            => left.resAmounts == right.resAmounts && left.rawMatsMix == right.rawMatsMix;

        public static bool operator !=(AllResAmounts left, AllResAmounts right)
            => !(left == right);

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

        public bool Equals(AllResAmounts other)
            => this == other;

        public override bool Equals(object? obj)
            => obj is AllResAmounts other && Equals(other: other);

        public override int GetHashCode()
            => HashCode.Combine(resAmounts, rawMatsMix);

        static AllResAmounts IMin<AllResAmounts>.Min(AllResAmounts left, AllResAmounts right)
            => new
            (
                MyMathHelper.Min(left.resAmounts, right.resAmounts),
                MyMathHelper.Min(left.rawMatsMix, right.rawMatsMix)
            );
    }

    //public readonly struct ResAmounts : IAmounts<ResAmounts>
    //{
    //    public static readonly ResAmounts empty;

    //    static ResAmounts IAdditiveIdentity<ResAmounts, ResAmounts>.AdditiveIdentity
    //        => empty;

    //    static ulong IMultiplicativeIdentity<ResAmounts, ulong>.MultiplicativeIdentity
    //        => 1;

    //    bool IFormOfEnergy<ResAmounts>.IsZero
    //        => IsEmpty();

    //    static ResAmounts()
    //    {
    //        empty = new(SomeResAmounts<RawMaterial>.empty, SomeResAmounts<RawMaterial>.empty, SomeResAmounts<Material>.empty, SomeResAmounts<Product>.empty);
    //    }

    //    private readonly SomeResAmounts<RawMaterial> rawMats, rawMatsMix;
    //    private readonly SomeResAmounts<Material> mats;
    //    private readonly SomeResAmounts<Product> prods;

    //    private ResAmounts(SomeResAmounts<RawMaterial> rawMats, SomeResAmounts<RawMaterial> rawMatsMix, SomeResAmounts<Material> mats, SomeResAmounts<Product> prods)
    //    {
    //        this.rawMats = rawMats;
    //        this.rawMatsMix = rawMatsMix;
    //        this.mats = mats;
    //        this.prods = prods;
    //    }

    //    public bool IsEmpty()
    //        => rawMats.IsEmpty() && rawMatsMix.IsEmpty() && mats.IsEmpty() && prods.IsEmpty();

    //    public static ResAmounts operator +(ResAmounts left, ResAmounts right)
    //        => new(left.rawMats + right.rawMats, left.rawMatsMix + right.rawMatsMix, left.mats + right.mats, left.prods + right.prods);

    //    public static ResAmounts operator -(ResAmounts left, ResAmounts right)
    //        => new(left.rawMats - right.rawMats, left.rawMatsMix - right.rawMatsMix, left.mats - right.mats, left.prods - right.prods);

    //    public static ResAmounts operator *(ResAmounts left, ulong right)
    //        => new(left.rawMats * right, left.rawMatsMix * right, left.mats * right, left.prods * right);

    //    public static ResAmounts operator *(ulong left, ResAmounts right)
    //        => right * left;

    //    public static bool operator ==(ResAmounts left, ResAmounts right)
    //        => left.rawMats == right.rawMats && left.rawMatsMix == right.rawMatsMix && left.mats == right.mats && left.prods == right.prods;

    //    public static bool operator !=(ResAmounts left, ResAmounts right)
    //        => !(left == right);

    //    public static explicit operator Energy(ResAmounts formOfEnergy)
    //        => (Energy)formOfEnergy.rawMats + (Energy)formOfEnergy.rawMatsMix + (Energy)formOfEnergy.mats + (Energy)formOfEnergy.prods;

    //    public static bool operator >=(ResAmounts left, ResAmounts right)
    //        => left.rawMats >= right.rawMats && left.rawMatsMix >= right.rawMatsMix && left.mats >= right.mats && left.prods >= right.prods;

    //    public static bool operator <=(ResAmounts left, ResAmounts right)
    //        => right >= left;

    //    static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator <(ResAmounts left, ResAmounts right)
    //        => left <= right && left != right;

    //    static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator >(ResAmounts left, ResAmounts right)
    //        => left >= right && left != right;

    //    public bool Equals(ResAmounts other)
    //        => this == other;

    //    public override bool Equals(object? obj)
    //        => obj is ResAmounts other && Equals(other: other);

    //    public override int GetHashCode()
    //        => HashCode.Combine(rawMats, rawMatsMix, mats, prods);

    //    static ResAmounts IMin<ResAmounts>.Min(ResAmounts left, ResAmounts right)
    //        => new
    //        (
    //            MyMathHelper.Min(left.rawMats, right.rawMats),
    //            MyMathHelper.Min(left.rawMatsMix, right.rawMatsMix),
    //            MyMathHelper.Min(left.mats, right.mats),
    //            MyMathHelper.Min(left.prods, right.prods)
    //        );
    //}
}
