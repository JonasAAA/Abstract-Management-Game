using Game1.Collections;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        public string Name { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }
        public Temperature MeltingPoint { get; }
        public ResRecipe Recipe { get; }

        public Material(string name, RawMatAmounts composition)
        {
            Name = name;
            Mass = composition.Mass();
            HeatCapacity = composition.HeatCapacity();
            Area = composition.Area();
            UsefulArea = Area;
            RawMatComposition = composition;

            MeltingPoint = ResAndIndustryAlgos.MaterialMeltingPoint(materialComposition: composition);

            // Need this before creating the recipe since to create SomeResAmounts you need all used resources to be registered first
            CurResConfig.AddRes(resource: this);

            Recipe = ResRecipe.CreateOrThrow
            (
                ingredients: composition.ToAll(),
                results: new AllResAmounts(res: this, amount: 1)
            );
        }
    }
}
