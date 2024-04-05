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
                    (prodParams: Product.productParamsDict["Gear"], amount: 4)
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
                        (prodParams: Product.productParamsDict["Gear"], amount: 4),
                        (prodParams: Product.productParamsDict["Wire"], amount: 1)
                    }.ToEfficientReadOnlyCollection()
                ),
                new MaterialProduction.GeneralBuildingParams
                (
                    nameVisual: UIAlgorithms.GetBasicMaterialProductionNameVisual,
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Gear"], amount: 5),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection()
                ),
                new Manufacturing.GeneralBuildingParams
                (
                    nameVisual: UIAlgorithms.GetBasicManufacturingNameVisual(prodParams: Product.productParamsDict["Gear"]),
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Gear"], amount: 3),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection(),
                    productParams: Product.productParamsDict["Gear"]
                ),
                new Manufacturing.GeneralBuildingParams
                (
                    nameVisual: UIAlgorithms.GetBasicManufacturingNameVisual(prodParams: Product.productParamsDict["Wire"]),
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Gear"], amount: 3),
                        (prodParams: Product.productParamsDict["Wire"], amount: 2)
                    }.ToEfficientReadOnlyCollection(),
                    productParams: Product.productParamsDict["Wire"]
                ),
                startingStorageParams,
                new Landfill.GeneralBuildingParams
                (
                    name: "Basic landfill",
                    energyPriority: averageEnergyPriority,
                    buildingComponentPropors: new List<(Product.Params prodParams, ulong amount)>()
                    {
                        (prodParams: Product.productParamsDict["Gear"], amount: 4),
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
        }
    }
}
