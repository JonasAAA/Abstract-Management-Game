namespace Game1.Industries
{
    [Serializable]
    public class IndustryConfig
    {
        public readonly ReadOnlyCollection<IBuildableParams> constrBuildingParams;

        public IndustryConfig()
        {
            constrBuildingParams = new(list: new IBuildableParams[]
            {
                new Construction.Params
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
                new Mining.Params
                (
                    name: "mine_lvl1",
                    energyPriority: new(value: 20),
                    reqSkillPerUnitSurface: (UDouble).1,
                    reqWattsPerUnitSurface: 10,
                    minedResPerUnitSurface: 10,
                    miningDuration: TimeSpan.FromSeconds(value: 2)
                ),
                new Construction.Params
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
                        demandPerUnitSurface: new()
                        {
                            [(ResInd)0] = 5,
                            [(ResInd)1] = 5
                        },
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    costPerUnitSurface: new()
                    {
                        [(ResInd)0] = 20,
                        [(ResInd)1] = 20
                    }
                ),
                new Construction.Params
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
                // TODO: consider deleting
                //new Construction.Params
                //(
                //    name: "factory costruction",
                //    energyPriority: new(value: 10),
                //    reqSkillPerUnitSurface: (UDouble).1,
                //    reqWattsPerUnitSurface: 100,
                //    industryParams: new Production.Params
                //    (
                //        name: "factory0_lvl2",
                //        energyPriority: new(value: 20),
                //        reqSkillPerUnitSurface: (UDouble).1,
                //        reqWattsPerUnitSurface: 10,
                //        supplyPerUnitSurface: new()
                //        {
                //            [(ResInd)0] = 100,
                //        },
                //        demandPerUnitSurface: new()
                //        {
                //            [(ResInd)1] = 50,
                //        },
                //        prodDuration: TimeSpan.FromSeconds(value: 2)
                //    ),
                //    duration: TimeSpan.FromSeconds(5),
                //    costPerUnitSurface: new()
                //    {
                //        [(ResInd)1] = 20,
                //        [(ResInd)2] = 10
                //    }
                //),
                new Construction.Params
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
