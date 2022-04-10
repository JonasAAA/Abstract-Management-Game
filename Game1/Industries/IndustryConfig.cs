﻿namespace Game1.Industries
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
                    energyPriority: new(value: 20),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 10,
                    industryParams: new House.Params
                    (
                        name: "house",
                        floorSpacePerUnitSurface: 100
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        supplyPerUnitSurface: new()
                        {
                            [(ResInd)0] = 10,
                        },
                        demandPerUnitSurface: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        supplyPerUnitSurface: new()
                        {
                            [(ResInd)1] = 10,
                        },
                        demandPerUnitSurface: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        supplyPerUnitSurface: new()
                        {
                            [(ResInd)2] = 10,
                        },
                        demandPerUnitSurface: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new
                (
                    name: "power plant costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).3,
                    reqWattsPerUnitSurface: 1,
                    industryParams: new PowerPlant.Params
                    (
                        name: "power_plant_lvl1",
                        reqSkillPerUnitSurface: (UDouble).2,
                        prodWattsPerUnitSurface: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 100,
                    industryParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
                        energyPriority: new(value: 20),
                        reqSkillPerUnitSurface: (UDouble).1,
                        reqWattsPerUnitSurface: 10,
                        supplyPerUnitSurface: new()
                        {
                            [(ResInd)0] = 100,
                        },
                        demandPerUnitSurface: new()
                        {
                            [(ResInd)1] = 50,
                        },
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                    {
                        [(ResInd)1] = 20,
                        [(ResInd)2] = 10
                    }
                ),
                new
                (
                    name: "reprod. ind. constr.",
                    energyPriority: new(value: 10),
                    reqSkillPerUnitSurface: (UDouble).05,
                    reqWattsPerUnitSurface: 200,
                    industryParams: new ReprodIndustry.Params
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
