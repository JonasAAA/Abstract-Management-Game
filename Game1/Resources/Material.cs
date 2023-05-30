using Game1.Collections;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Material : IResource
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public Area Area { get; }
        public RawMaterialsMix RawMatComposition { get; }
        public Temperature MeltingPoint { get; }

        public Material(RawMaterialsMix composition)
        {
            Mass = composition.Mass();
            HeatCapacity = composition.HeatCapacity();
            Area = composition.Area();
            RawMatComposition = composition;

            MeltingPoint = ResAndIndustryAlgos.MaterialMeltingPoint(materialComposition: composition);

            CurResConfig.AddRes(resource: this);
        }
    }
}
