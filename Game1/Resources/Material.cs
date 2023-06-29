using Game1.Collections;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public AreaInt UsefulArea { get; }
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }
        public Temperature MeltingPoint { get; }

        public Material(SomeResAmounts<RawMaterial> composition)
        {
            Mass = composition.Mass();
            HeatCapacity = composition.HeatCapacity();
            Area = composition.Area();
            UsefulArea = Area;
            RawMatComposition = composition;

            MeltingPoint = ResAndIndustryAlgos.MaterialMeltingPoint(materialComposition: composition);

            CurResConfig.AddRes(resource: this);
        }
    }
}
