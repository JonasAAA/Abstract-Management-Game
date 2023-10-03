using Game1.Collections;

namespace Game1.Resources
{
    public interface IProductClass : IHasToString
    {
        [Serializable]
        private sealed class Mechanical : IProductClass
        {
            public Propor ProportionOfMassMovingWithMechanicalProduction
                => (Propor).8;

            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [IMaterialPurpose.mechanical] = 1
                };

            public sealed override string ToString()
                => "Mechanical";
        }

        [Serializable]
        private sealed class Electronics : IProductClass
        {
            public Propor ProportionOfMassMovingWithMechanicalProduction
                => (Propor).5;

            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [IMaterialPurpose.electricalConductor] = 1,
                    [IMaterialPurpose.electricalInsulator] = 1
                };

            public sealed override string ToString()
                => "Electronics";
        }

        [Serializable]
        private sealed class Roof : IProductClass
        {
            public Propor ProportionOfMassMovingWithMechanicalProduction
                => (Propor)0;

            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [IMaterialPurpose.roofSurface] = 1
                };

            public sealed override string ToString()
                => "Roof";
        }

        public static readonly IProductClass mechanical = new Mechanical();
        public static readonly IProductClass electronics = new Electronics();
        public static readonly IProductClass roof = new Roof();

        // DON'T forget to put all material purposes in this list.
        // There is a test to check that
        public static readonly EfficientReadOnlyCollection<IProductClass> all = new List<IProductClass>() { mechanical, electronics, roof }.ToEfficientReadOnlyCollection();

        public Propor ProportionOfMassMovingWithMechanicalProduction { get; }

        public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount { get; }

        public sealed void ThrowIfWrongIMatPurposeSet(EfficientReadOnlyDictionary<IMaterialPurpose, Material> materialChoices)
            => MatPurposeToAmount.Keys.ToHashSet().SetEquals(materialChoices.Keys);
    }
}
