using Game1.Collections;

namespace Game1.Resources
{
    public interface IProductClass
    {
        [Serializable]
        public readonly struct Mechanical : IProductClass
        {
            public static string Name
                => "Mechanical";

            public static EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToMultipleOfMatTargetAreaDivisor
                => new()
                {
                    [IMaterialPurpose.mechanical] = 1
                };
        }

        [Serializable]
        private sealed class Electronics : IProductClass
        {
            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToMultipleOfMatTargetAreaDivisor
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
            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToMultipleOfMatTargetAreaDivisor
                => new()
                {
                    [IMaterialPurpose.roofSurface] = 1
                };

            public sealed override string ToString()
                => "Roof";
        }

        //public static readonly IProductClass mechanical = new Mechanical();
        //public static readonly IProductClass electronics = new Electronics();
        //public static readonly IProductClass roof = new Roof();

        //// DON'T forget to put all material purposes in this list.
        //// There is a test to check that
        //public static readonly EfficientReadOnlyCollection<IProductClass> all = new List<IProductClass>() { mechanical, electronics, roof }.ToEfficientReadOnlyCollection();

        public static abstract string Name { get; }

        public static abstract EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToMultipleOfMatTargetAreaDivisor { get; }
    }
}
