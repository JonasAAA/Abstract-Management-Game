using Game1.Collections;
using Game1.UI;

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
            EnergyPriority constrEnergyPriority = new(value: 50),
                averageEnergyPriority = new(value: 20);
            startingPowerPlantParams = new PowerPlant.GeneralBuildingParams
            (
                name: "Basic Power Plant",
                buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                {
                    (prodParams: Product.productParamsDict["Wire"], amount: 4),
                    (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                }.ToEfficientReadOnlyCollection()
            );

            startingStorageParams = new Storage.GeneralBuildingParams
            (
                name: "Basic Storage",
                buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                {
                    (prodParams: Product.productParamsDict["Gear"], amount: 4),
                    (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                }.ToEfficientReadOnlyCollection()
            );

            List<Construction.GeneralParams> constrGeneralParamsIncompleteList =
            [
                new
                (
                    buildingGeneralParams: startingStorageParams,
                    energyPriority: constrEnergyPriority
                ),
                new
                (
                    buildingGeneralParams: startingPowerPlantParams,
                    energyPriority: constrEnergyPriority
                ),
                new
                (
                    buildingGeneralParams: new Mining.GeneralBuildingParams
                    (
                        name: "Basic Mining",
                        energyPriority: averageEnergyPriority,
                        buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                        {
                            (prodParams: Product.productParamsDict["Gear"], amount: 4),
                            (prodParams: Product.productParamsDict["Wire"], amount: 1),
                            (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: constrEnergyPriority
                ),
                new
                (
                    buildingGeneralParams: new Landfill.GeneralBuildingParams
                    (
                        name: "Basic landfill",
                        energyPriority: averageEnergyPriority,
                        buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                        {
                            (prodParams: Product.productParamsDict["Gear"], amount: 4),
                            (prodParams: Product.productParamsDict["Wire"], amount: 2),
                            (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: constrEnergyPriority
                ),
                new
                (
                    buildingGeneralParams: new MaterialProduction.GeneralBuildingParams
                    (
                        name: "Basic material production",
                        energyPriority: averageEnergyPriority,
                        buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                        {
                            (prodParams: Product.productParamsDict["Gear"], amount: 5),
                            (prodParams: Product.productParamsDict["Wire"], amount: 2),
                            (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                        }.ToEfficientReadOnlyCollection()
                    ),
                    energyPriority: constrEnergyPriority
                )
            ];
            constrGeneralParamsList = constrGeneralParamsIncompleteList.Concat
            (
                Product.productParamsDict.Select
                (
                    prodNameAndParams => new Construction.GeneralParams
                    (
                        buildingGeneralParams: new Manufacturing.GeneralBuildingParams
                        (
                            name: UIAlgorithms.ManufacturingBasicName(prodParamsName: prodNameAndParams.Key),
                            energyPriority: averageEnergyPriority,
                            buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                            {
                                (prodParams: Product.productParamsDict["Gear"], amount: 3),
                                (prodParams: Product.productParamsDict["Wire"], amount: 2),
                                (prodParams: Product.productParamsDict["Roof Tile"], amount: 1)
                            }.ToEfficientReadOnlyCollection(),
                            productParams: prodNameAndParams.Value
                        ),
                        energyPriority: constrEnergyPriority
                    )
                )
            ).ToEfficientReadOnlyCollection();
#warning Complete this by making it configurable, if possible
        }
    }
}
