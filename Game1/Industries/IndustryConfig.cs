using Game1.Collections;

namespace Game1.Industries
{
    [Serializable]
    public sealed class IndustryConfig
    {
        public readonly EfficientReadOnlyCollection<Construction.GeneralParams> constrGeneralParamsList;
        //public readonly House.Factory basicHouseFactory;
        //public readonly PowerPlant.Factory basicPowerPlantFactory;

        public IndustryConfig()
        {
            constrGeneralParamsList = new()
            {

            };
#warning Complete this
        }
    }
}
