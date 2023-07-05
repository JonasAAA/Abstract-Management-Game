using Game1.Collections;

namespace Game1.Industries
{
    [Serializable]
    public sealed class IndustryConfig
    {
        public readonly EfficientReadOnlyCollection<IBuildableFactory> constrBuildingParams;
        //public readonly House.Factory basicHouseFactory;
        //public readonly PowerPlant.Factory basicPowerPlantFactory;

        public IndustryConfig()
        {
            constrBuildingParams = new();
#warning Complete this
        }
    }
}
