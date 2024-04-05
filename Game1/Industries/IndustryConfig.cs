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
                    (prodParams: Product.productParamsDict["Wire"], amount: 4)
                }.ToEfficientReadOnlyCollection()
            );

            startingStorageParams = new Storage.GeneralBuildingParams
            (
                name: "Basic Storage",
                buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                {
                    (prodParams: Product.productParamsDict["Beam"], amount: 4)
                }.ToEfficientReadOnlyCollection()
            );

            List<IGeneralBuildingConstructionParams> generalBuildingConstructionParamsList =
            [
                new Mining.GeneralBuildingParams
                (
                    name: "Basic Mining",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 4),
                        (prodParams: Product.productParamsDict["Wire"], amount: 1)
                    }.ToEfficientReadOnlyCollection()
                ),
                new MaterialProduction.GeneralBuildingParams
                (
                    nameVisual: UIAlgorithms.GetBasicMaterialProductionNameVisual,
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 5),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection()
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Beam",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 3),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Wire",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 3),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }
                ),
                startingStorageParams,
                new Landfill.GeneralBuildingParams
                (
                    name: "Basic landfill",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 4),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection()
                ),
                startingPowerPlantParams,
                new LightRedirection.GeneralBuildingParams
                (
                    name: "Basic light redirection",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection()
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Screw",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 2),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Circuit",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 2),
                        (prodParams: Product.productParamsDict["Wire"], amount: 3)
                    }
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Gear",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Beam"], amount: 2),
                        (prodParams: Product.productParamsDict["Screw"], amount: 3),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }
                ),
                CreateManufacturingBuildingParams
                (
                    productName: "Processor",
                    buildingComponentPropors: new()
                    {
                        (prodParams: Product.productParamsDict["Gear"], amount: 2),
                        (prodParams: Product.productParamsDict["Wire"], amount: 1),
                        (prodParams: Product.productParamsDict["Circuit"], amount: 4)
                    }
                ),
                new Mining.GeneralBuildingParams
                (
                    name: "Electronic Mining",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Wire"], amount: 1),
                        (prodParams: Product.productParamsDict["Circuit"], amount: 2),
                        (prodParams: Product.productParamsDict["Processor"], amount: 3),
                    }.ToEfficientReadOnlyCollection()
                ),
                new Landfill.GeneralBuildingParams
                (
                    name: "Electronic Landfill",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Wire"], amount: 4),
                        (prodParams: Product.productParamsDict["Processor"], amount: 3),
                    }.ToEfficientReadOnlyCollection()
                ),
            ];
            constrGeneralParamsList = generalBuildingConstructionParamsList.Select
            (
                buildingGeneralParams => new Construction.GeneralParams
                (
                    buildingGeneralParams: buildingGeneralParams,
                    energyPriority: constrEnergyPriority
                )
            ).ToEfficientReadOnlyCollection();
#warning Complete this by making it configurable, if possible

            Manufacturing.GeneralBuildingParams CreateManufacturingBuildingParams(string productName, List<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
                => new
                (
                    nameVisual: UIAlgorithms.GetBasicManufacturingNameVisual(prodParams: Product.productParamsDict[productName]),
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: buildingComponentPropors.ToEfficientReadOnlyCollection(),
                    productParams: Product.productParamsDict[productName]
                );
        }
    }
}
