using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class IndustryConfig
    {
        public readonly ReadOnlyCollection<IBuildableFactory> constrBuildingParams;
        public readonly House.Factory basicHouseFactory;
        public readonly PowerPlant.Factory basicPowerPlantFactory;

        public IndustryConfig()
        {
            basicHouseFactory = new House.Factory
            (
                name: "house",
                floorSpacePerUnitSurface: 1,
                buildingCostPerUnitSurface: new()
                {
                    [(ResInd)0] = 1
                }
            );

            basicPowerPlantFactory = new PowerPlant.Factory
            (
                name: "power_plant_lvl1",
                reqSkillPerUnitSurface: (UDouble).2,
                surfaceAbsorbtionPropor: (Propor).2,
                buildingCostPerUnitSurface: new()
                {
                    [(ResInd)0] = 1
                }
            );

            constrBuildingParams = new(list: new IBuildableFactory[]
            {
                new Construction.Factory
                (
                    name: "house construction",
                    energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 10,
                    industryFactory: basicHouseFactory,
                    duration: TimeSpan.FromSeconds(5)
                ),
                new Mining.Factory
                (
                    name: "mine_lvl1",
                    energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 1,
                    minedResPerUnitSurfacePerSec: (UDouble)1
                ),
                new PlanetEnlargement.Factory
                (
                    name: "planet_enlargement_lvl1",
                    energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).2,
                    reqWattsPerUnitSurface: 5,
                    addedResPerUnitSurfacePerSec: (UDouble)2
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: CurWorldConfig.industryConstructionEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory2_lvl1",
                        baseResRecipe: CurResConfig.resources[(NonBasicResInd)2].Recipe,
                        prodResPerUnitSurface: 1,
                        energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2),
                        buildingCostPerUnitSurface: new()
                        {
                            [(ResInd)0] = 2,
                            [(ResInd)1] = 2
                        }
                    ),
                    duration: TimeSpan.FromSeconds(5)
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: CurWorldConfig.industryConstructionEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory3_lvl1",
                        baseResRecipe: CurResConfig.resources[(NonBasicResInd)3].Recipe,
                        prodResPerUnitSurface: 1,
                        energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2),
                        buildingCostPerUnitSurface: new()
                        {
                            [(ResInd)0] = 2,
                            [(ResInd)1] = 2
                        }
                    ),
                    duration: TimeSpan.FromSeconds(5)
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: CurWorldConfig.industryConstructionEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory4_lvl1",
                        baseResRecipe: CurResConfig.resources[(NonBasicResInd)4].Recipe,
                        prodResPerUnitSurface: 1,
                        energyPriority: CurWorldConfig.industryOperationEnergyPrior,
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2),
                        buildingCostPerUnitSurface: new()
                        {
                            [(ResInd)0] = 2,
                            [(ResInd)1] = 2
                        }
                    ),
                    duration: TimeSpan.FromSeconds(5)
                ),
                new Construction.Factory
                (
                    name: "power plant costruction",
                    energyPriority: CurWorldConfig.industryConstructionEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).3,
                    reqWattsPerUnitSurface: (UDouble).05,
                    industryFactory: basicPowerPlantFactory,
                    duration: TimeSpan.FromSeconds(5)
                ),
                new Construction.Factory
                (
                    name: "reprod. ind. constr.",
                    energyPriority: CurWorldConfig.industryConstructionEnergyPrior,
                    reqSkillPerUnitSurface: (UDouble).05,
                    reqWattsPerUnitSurface: 10,
                    industryFactory: new ReprodIndustry.Factory
                    (
                        name: "reprod. ind.",
                        energyPriority: CurWorldConfig.reprodIndustryOperationEnergyPrior,
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerChild: 100,
                        maxCouplesPerUnitSurface: (UDouble).1,
                        birthDuration: TimeSpan.FromSeconds(1),
                        buildingCostPerUnitSurface: new()
                        {
                            [(ResInd)0] = 1
                        }
                    ),
                    duration: TimeSpan.FromSeconds(20)
                ),
            });
        }
    }
}
