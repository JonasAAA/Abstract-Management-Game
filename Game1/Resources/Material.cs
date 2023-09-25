using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        public static Material CreateAndAddToCurResConfig(string name, RawMatAmounts rawMatAreaPropors)
            // Material consrutor adds itself to CurResConfig
            => new
            (
                name: name,
                composition: ResAndIndustryAlgos.CreateRawMatCompositionFromRawMatPropors(rawMatPropors: rawMatAreaPropors)
            );

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public ResRecipe Recipe { get; }

        private readonly string name;

        private Material(string name, RawMatAmounts composition)
        {
            this.name = name;
            Mass = composition.Mass();
            HeatCapacity = composition.HeatCapacity();
            Area = ResAndIndustryAlgos.blockArea;
            Debug.Assert(Area == composition.Area());
            RawMatComposition = composition;
            Debug.Assert(RawMatComposition.All(rawMatAmount => rawMatAmount.amount % ResAndIndustryAlgos.materialCompositionDivisor is 0));

            // Need this before creating the recipe since to create SomeResAmounts you need all used resources to be registered first
            CurResConfig.AddRes(resource: this);

            Recipe = ResRecipe.CreateOrThrow
            (
                ingredients: composition.ToAll(),
                results: new AllResAmounts(res: this, amount: 1)
            );
        }

        public override string ToString()
            => name;
    }
}
