using Game1.PrimitiveTypeWrappers;

namespace Game1.Industries
{
    [Serializable]
    public class IndustryConfig
    {
        public readonly ReadOnlyCollection<Construction.Params> constrBuildingParams;

        public IndustryConfig()
        {
            constrBuildingParams = new(list: new Construction.Params[]
            {
                new
                (
                    name: "house construction",
                    energyPriority: 20,
                    reqSkillPerUnitSurface: (UFloat).1,
                    reqWattsPerUnitSurface: 10,
                    industryParams: new House.Params
                    (
                        name: "house",
                        floorSpacePerUnitSurface: 100
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
                        energyPriority: 20,
                        reqSkillPerUnitSurface: (UFloat).1,
                        reqWattsPerUnitSurface: 10,
                        supply: new()
                        {
                            [0] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
                        energyPriority: 20,
                        reqSkillPerUnitSurface: (UFloat).1,
                        reqWattsPerUnitSurface: 10,
                        supply: new()
                        {
                            [1] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
                        energyPriority: 20,
                        reqSkillPerUnitSurface: (UFloat).1,
                        reqWattsPerUnitSurface: 10,
                        supply: new()
                        {
                            [2] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "power plant costruction",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).3,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new PowerPlant.Params
                    (
                        name: "power_plant_lvl1",
                        reqSkillPerUnitSurface: (UFloat).2,
                        prodWattsPerUnitSurface: 1000
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
                        energyPriority: 20,
                        reqSkillPerUnitSurface: (UFloat).1,
                        reqWattsPerUnitSurface: 10,
                        supply: new()
                        {
                            [0] = 100,
                        },
                        demand: new()
                        {
                            [1] = 50,
                        },
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                    {
                        [1] = 20,
                        [2] = 10
                    }
                ),
                new
                (
                    name: "reprod. ind. constr.",
                    energyPriority: 10,
                    reqSkillPerUnitSurface: (UFloat).05,
                    reqWattsPerUnitSurface: 200,
                    industryParams: new ReprodIndustry.Params
                    (
                        name: "reprod. ind.",
                        energyPriority: 11,
                        reqSkillPerUnitSurface: (UFloat).1,
                        reqWattsPerChild: 10,
                        maxCouplesPerUnitSurface: 10,
                        resPerChild: new()
                        {
                            [0] = 10,
                        },
                        birthDuration: TimeSpan.FromSeconds(1)
                    ),
                    duration: TimeSpan.FromSeconds(20),
                    cost: new()
                ),
            });
        }
    }
}
