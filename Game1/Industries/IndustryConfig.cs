using Game1.Collections;

namespace Game1.Industries
{
    [Serializable]
    public sealed class IndustryConfig
    {
        public readonly EfficientReadOnlyCollection<Construction.GeneralParams> constrGeneralParamsList;
        public readonly PowerPlant.GeneralBuildingParams startingPowerPlantParams;
        public readonly Storage.GeneralBuildingParams startingStorageParams;

        public IndustryConfig()
        {
            startingPowerPlantParams = new PowerPlant.GeneralBuildingParams
            (
                name: "Basic Power Plant",
                buildingComponentPropors: new List<(IProduct.IParams prodParams, ulong amount)>()
                {
                    (prodParams: IProduct.productParamsDict["Wire"], amount: 4),
                    (prodParams: IProduct.productParamsDict["Roof Tile"], amount: 1)
                }.ToEfficientReadOnlyCollection()
            );

            startingStorageParams = new Storage.GeneralBuildingParams
            (
                name: "Basic Storage",
                buildingComponentPropors: new List<(IProduct.IParams prodParams, ulong amount)>()
                {
                    (prodParams: IProduct.productParamsDict["Gear"], amount: 4),
                    (prodParams: IProduct.productParamsDict["Roof Tile"], amount: 1)
                }.ToEfficientReadOnlyCollection()
            );

            constrGeneralParamsList = new List<Construction.GeneralParams>()
            {
                new
                (
                    buildingGeneralParams: startingStorageParams,
                    energyPriority: new(value: 50)
                ),
                new
                (
                    buildingGeneralParams: startingPowerPlantParams,
                    energyPriority: new(value: 50)
                ),
                new
                (
                    buildingGeneralParams: new Mining.GeneralBuildingParams
                    (
                        name: "Basic Mining",
                        energyPriority: new(value: 20),
                        buildingComponentPropors: new List<(IProduct.IParams prodParams, ulong amount)>()
                        {
                            (prodParams: IProduct.productParamsDict["Gear"], amount: 4),
                            (prodParams: IProduct.productParamsDict["Wire"], amount: 1),
                            (prodParams: IProduct.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: new(value: 50)
                ),
            }.ToEfficientReadOnlyCollection();
#warning Complete this by making it configurable, if possible
        }
    }
}
