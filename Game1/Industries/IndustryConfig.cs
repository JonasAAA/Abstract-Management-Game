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
            constrGeneralParamsList = new List<Construction.GeneralParams>()
            {
                new
                (
                    buildingGeneralParams: new PowerPlant.GeneralBuildingParams
                    (
                        name: "Basic Power Plant",
                        buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                        {
                            (prodParams: Product.productParamsDict["Wire"], amount: 4),
                            (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: new(value: 50)
                ),
                new
                (
                    buildingGeneralParams: new Mining.GeneralBuildingParams
                    (
                        name: "Basic Mining",
                        energyPriority: new(value: 20),
                        buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                        {
                            (prodParams: Product.productParamsDict["Gear"], amount: 4),
                            (prodParams: Product.productParamsDict["Wire"], amount: 1),
                            (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: new(value: 50)
                ),
            }.ToEfficientReadOnlyCollection();
#warning Complete this by making it configurable, if possible
        }
    }
}
