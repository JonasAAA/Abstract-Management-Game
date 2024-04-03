using Game1.ContentNames;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.GlobalTypes.GameConfig;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        public static Material CreateAndAddToCurResConfig(string name, TextureName iconName, RawMatAmounts rawMatAreaPropors)
            // Material consrutor adds itself to CurResConfig
            => new
            (
                name: name,
                iconName: iconName,
                composition: ResAndIndustryAlgos.CreateMatCompositionFromRawMatPropors(rawMatPropors: rawMatAreaPropors)
            );

        public ConfigurableIcon Icon { get; }
        public ConfigurableIcon SmallIcon { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public ResRecipe Recipe { get; }

        private readonly string name;

        private Material(string name, TextureName iconName, RawMatAmounts composition)
        {
            this.name = name;
            Icon = new Icon(name: iconName, height: CurGameConfig.iconHeight).WithDefaultBackgroundColor();
            SmallIcon = new Icon(name: iconName, height: CurGameConfig.smallIconHeight).WithDefaultBackgroundColor();
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
