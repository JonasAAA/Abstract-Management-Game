using Game1.Collections;

namespace Game1.Resources
{
    // If make ProductClass or MaterialPurpose into a (record) struct, some initialization failure happens and
    // System.TypeLoadException exception is thrown. Couldn't figure out how to fix that, so both of those will be reference types for now.
    [Serializable]
    public class ProductClass : IEquatable<ProductClass>
    {
        public static readonly ProductClass mechanical = new
        (
            name: "Mechanical",
            matPurposeToAmount: new()
            {
                [MaterialPurpose.mechanical] = 1
            }
        );
        public static readonly ProductClass electronics = new
        (
            name: "Electronics",
            matPurposeToAmount: new()
            {
                [MaterialPurpose.electricalConductor] = 1,
                [MaterialPurpose.electricalInsulator] = 1
            }
        );
        public static readonly ProductClass roof = new
        (
            name: "Roof",
            matPurposeToAmount: new()
            {
                [MaterialPurpose.roofSurface] = 1
            }
        );

        // DON'T forget to put all material purposes in this list.
        // There is a test to check that
        public static readonly EfficientReadOnlyCollection<ProductClass> all = new List<ProductClass>() { mechanical, electronics, roof }.ToEfficientReadOnlyCollection();

        public readonly EfficientReadOnlyDictionary<MaterialPurpose, ulong> matPurposeToAmount;

        private readonly string name;

        private ProductClass(string name, EfficientReadOnlyDictionary<MaterialPurpose, ulong> matPurposeToAmount)
        {
            this.name = name;
            this.matPurposeToAmount = matPurposeToAmount;
        }

        public void ThrowIfWrongIMatPurposeSet(EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices)
            => matPurposeToAmount.Keys.ToHashSet().SetEquals(materialChoices.Keys);

        public T SwitchExpression<T>(Func<T> mechanical, Func<T> electronics, Func<T> roof)
        {
            if (this == ProductClass.mechanical)
                return mechanical();
            if (this == ProductClass.electronics)
                return electronics();
            if (this == ProductClass.roof)
                return roof();
            throw new InvalidStateException();
        }

        public override string ToString()
            => name;

        public bool Equals(ProductClass other)
            => this == other;

        public override bool Equals(object? obj)
            => obj is ProductClass prodClass && Equals(prodClass);

        public static bool operator ==(ProductClass left, ProductClass right)
            => left.name == right.name;

        public static bool operator !=(ProductClass left, ProductClass right)
            => !(left == right);

        public override int GetHashCode()
            => string.GetHashCode(name, StringComparison.Ordinal);
    }
}
