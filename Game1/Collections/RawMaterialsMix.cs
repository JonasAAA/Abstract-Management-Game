using System.Collections;
using System.Numerics;

namespace Game1.Collections
{
    public readonly record struct RawMaterialsMix : IResAmounts<RawMaterialsMix>, IEnumerable<ResAmount<RawMaterial>>
    {
        public static readonly RawMaterialsMix empty;

        static RawMaterialsMix IAdditiveIdentity<RawMaterialsMix, RawMaterialsMix>.AdditiveIdentity
            => empty;

        static ulong IMultiplicativeIdentity<RawMaterialsMix, ulong>.MultiplicativeIdentity
            => 1;

        static RawMaterialsMix()
            => empty = new(rawMatsMix: SomeResAmounts<RawMaterial>.empty);

        public bool IsEmpty
            => rawMatsMix.IsEmpty;

        public Color Color
            => throw new NotImplementedException();

        bool IFormOfEnergy<RawMaterialsMix>.IsZero
            => IsEmpty;

        private readonly SomeResAmounts<RawMaterial> rawMatsMix;

        public RawMaterialsMix()
            : this(rawMatsMix: SomeResAmounts<RawMaterial>.empty)
        { }

        public RawMaterialsMix(SomeResAmounts<RawMaterial> rawMatsMix)
            => this.rawMatsMix = rawMatsMix;

        public ulong this[RawMaterial rawMat]
            => rawMatsMix[rawMat];

        public ResRecipe SplittingRecipe()
            => ResRecipe.CreateOrThrow(ingredients: this.ToAll(), results: rawMatsMix.Generalize().ToAll());

        public Mass Mass()
            => rawMatsMix.Mass();

        public HeatCapacity HeatCapacity()
            => rawMatsMix.HeatCapacity();

        public AreaInt Area()
            => rawMatsMix.Sum(resAmount => resAmount.res.Area * resAmount.amount);

        public RawMaterialsMix RawMatComposition()
        => this;

        public static explicit operator Energy(RawMaterialsMix formOfEnergy)
            => (Energy)formOfEnergy.rawMatsMix;

        static RawMaterialsMix IMin<RawMaterialsMix>.Min(RawMaterialsMix left, RawMaterialsMix right)
            => new(MyMathHelper.Min(left.rawMatsMix, right.rawMatsMix));
    
        public static RawMaterialsMix operator +(RawMaterialsMix left, RawMaterialsMix right)
            => new(left.rawMatsMix + right.rawMatsMix);

        public static RawMaterialsMix operator -(RawMaterialsMix left, RawMaterialsMix right)
            => new(left.rawMatsMix - right.rawMatsMix);

        public static RawMaterialsMix operator *(RawMaterialsMix left, ulong right)
            => new(left.rawMatsMix * right);

        public static RawMaterialsMix operator *(ulong left, RawMaterialsMix right)
            => right * left;

        public static bool operator >=(RawMaterialsMix left, RawMaterialsMix right)
            => left.rawMatsMix >= right.rawMatsMix;

        public static bool operator <=(RawMaterialsMix left, RawMaterialsMix right)
            => left.rawMatsMix <= right.rawMatsMix;

        static bool IComparisonOperators<RawMaterialsMix, RawMaterialsMix, bool>.operator >(RawMaterialsMix left, RawMaterialsMix right)
            => left >= right && left != right;

        static bool IComparisonOperators<RawMaterialsMix, RawMaterialsMix, bool>.operator <(RawMaterialsMix left, RawMaterialsMix right)
            => left <= right && left != right;

        public IEnumerator<ResAmount<RawMaterial>> GetEnumerator()
            => rawMatsMix.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
