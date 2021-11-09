using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Game1.Industries
{
    [DataContract]
    public class IndustryConfig
    {
        [DataMember] public readonly ReadOnlyCollection<Construction.Params> constrBuildingParams;

        public IndustryConfig()
        {
            constrBuildingParams = new(list: new Construction.Params[]
            {
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkill: 10,
                    reqWatts: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
                        energyPriority: 20,
                        reqSkill: 10,
                        reqWatts: 10,
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
                    reqSkill: 10,
                    reqWatts: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
                        energyPriority: 20,
                        reqSkill: 10,
                        reqWatts: 10,
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
                    reqSkill: 10,
                    reqWatts: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
                        energyPriority: 20,
                        reqSkill: 10,
                        reqWatts: 10,
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
                    reqSkill: 30,
                    reqWatts: 100,
                    industrParams: new PowerPlant.Params
                    (
                        name: "power_plant_lvl1",
                        reqSkill: 20,
                        prodWatts: 1000
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    energyPriority: 10,
                    reqSkill: 10,
                    reqWatts: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
                        energyPriority: 20,
                        reqSkill: 10,
                        reqWatts: 10,
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
                    reqSkill: 5,
                    reqWatts: 200,
                    industrParams: new ReprodIndustry.Params
                    (
                        name: "reprod. ind.",
                        energyPriority: 11,
                        reqSkill: 10,
                        reqWattsPerChild: 10,
                        maxCouples: 10,
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
