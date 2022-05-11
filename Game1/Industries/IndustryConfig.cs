namespace Game1.Industries
{
    [Serializable]
    public class IndustryConfig
    {
        public readonly ReadOnlyCollection<IBuildableFactory> constrBuildingParams;

        public IndustryConfig()
        {
            constrBuildingParams = new(list: new IBuildableFactory[]
            {
                new Construction.Factory
                (
                    name: "house construction",
                    energyPriority: new(value: 20),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 10,
                    industryFactory: new House.Factory
                    (
                        name: "house",
                        floorSpacePerUnitSurface: 100
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new Mining.Factory
                (
                    name: "mine_lvl1",
                    energyPriority: new(value: 20),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 10,
                    minedResPerUnitSurfacePerSec: (UDouble)1
                ),
                new PlanetEnlargement.Factory
                (
                    name: "planet_enlargement_lvl1",
                    energyPriority: new(value: 20),
                    reqSkillPerUnitSurface: (UDouble).2,
                    reqWattsPerUnitSurface: 5,
                    addedResPerUnitSurfacePerSec: (UDouble)2
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory2_lvl1",
                        producedResInd: (NonBasicResInd)2,
                        prodResPerUnitSurface: 1,
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                    {
                        [(ResInd)0] = 2,
                        [(ResInd)1] = 2
                    }
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory3_lvl1",
                        producedResInd: (NonBasicResInd)3,
                        prodResPerUnitSurface: 1,
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                    {
                        [(ResInd)0] = 2,
                        [(ResInd)1] = 2
                    }
                ),
                new Construction.Factory
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryFactory: new Manufacturing.Factory
                    (
                        name: "factory4_lvl1",
                        producedResInd: (NonBasicResInd)4,
                        prodResPerUnitSurface: 1,
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                    {
                        [(ResInd)0] = 2,
                        [(ResInd)1] = 2
                    }
                ),
                new Construction.Factory
                (
                    name: "power plant costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).3,
                    reqWattsPerUnitSurface: 1,
                    industryFactory: new PowerPlant.Factory
                    (
                        name: "power_plant_lvl1",
                        reqSkillPerUnitSurface: (UDouble).2,
                        prodWattsPerUnitSurface: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new Construction.Factory
                (
                    name: "reprod. ind. constr.",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).05,
                    reqWattsPerUnitSurface: 200,
                    industryFactory: new ReprodIndustry.Factory
                    (
                        name: "reprod. ind.",
                        energyPriority: new(value: 11),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerChild: 10,
                        maxCouplesPerUnitSurface: 1,
                        resPerChild: new()
                        {
                            [(ResInd)0] = 10,
                        },
                        birthDuration: TimeSpan.FromSeconds(1)
                    ),
                    duration: TimeSpan.FromSeconds(20),
                    costPerUnitSurface: new()
                ),
            });
        }
    }
}
