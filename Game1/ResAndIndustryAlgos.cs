﻿using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        public static string RawMaterialName(ulong ind)
            => ind switch
            {
                0 => "firstium",
                1 => "secondium",
                2 => "thirdium",
                3 => "fourthium",
                4 => "fifthium",
                5 => "sixthium",
                > maxRawMatInd => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Primitive material is material composed from single raw material
        /// </summary>
        public static string PrimitiveMaterialName(ulong rawMatInd)
            => $"{RawMaterialName(rawMatInd)} material";

        // Want max density to be 1
        public static readonly AreaInt rawMaterialArea = AreaInt.CreateFromMetSq(valueInMetSq: RawMaterialMass(ind: 0).valueInKg);

        // Formula is like this for the following reasons:
        // * Want max mass to be quite small in order to have larger amount of blocks in the world
        // * Want mass to decrease with ind
        // * Want subsequent raw mat masses to have not-so-close of a ratio
        // CHANGING masses would still get the same amount of energy released in fusion per unit area, ASSUMING that gravity stays the same
        public static Mass RawMaterialMass(ulong ind)
            => Mass.CreateFromKg
            (
                ind switch
                {
                    0 => 12,
                    1 => 8,
                    2 => 6,
                    3 => 4,
                    4 => 3,
                    5 => 2,
                    > maxRawMatInd => throw new ArgumentOutOfRangeException()
                }
            );

        // The same area of stuff will always have thse same heat capacity
        // As said in https://en.wikipedia.org/wiki/Specific_heat_capacity#Monatomic_gases
        // heat capacity per mole is the same for all monatomic gases
        // That's because the atoms have nowhere else to store energy, other than in kinetic energy (and hence temperature)
        public static HeatCapacity RawMaterialHeatCapacity(ulong ind)
#warning Make this more interesting?
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        public const ulong energyInJPerKgOfMass = 1;

        public const ulong maxRawMatInd = 5;

        public const ulong maxProductIndInClass = 9;

        // Means all products must be from 1, 2, 3, or 6 components (some of which could be the same)
        public const ulong productRecipeInputAmountMultiple = 6;

        public static readonly ulong materialCompositionDivisor = ProductRawMatCompositionDivisor(prodIndInClass: 0) * productRecipeInputAmountMultiple;

        public static ulong ProductRawMatCompositionDivisor(ulong prodIndInClass)
            => MyMathHelper.Pow(productRecipeInputAmountMultiple, maxProductIndInClass - prodIndInClass);

        public static readonly AreaInt blockArea = rawMaterialArea * 100 * materialCompositionDivisor;

        private const ulong temperatureScaling = 10000;

        public static Temperature CalculateTemperature(HeatEnergy heatEnergy, HeatCapacity heatCapacity)
            => Temperature.CreateFromK(valueInK: temperatureScaling * (UDouble)heatEnergy.ValueInJ() / heatCapacity.valueInJPerK);

        public static HeatEnergy HeatEnergyFromTemperature(Temperature temperature, HeatCapacity heatCapacity)
            => HeatEnergy.CreateFromJoules(valueInJ: MyMathHelper.Round(heatCapacity.valueInJPerK * temperature.valueInK / temperatureScaling));

        public static RawMatAmounts CreateMatCompositionFromRawMatPropors(RawMatAmounts rawMatPropors)
        {
            ulong totalWeights = rawMatPropors.Sum(resAmount => resAmount.amount);
            if (totalWeights is 0)
                throw new ArgumentException();
            if (totalWeights > 20)
                throw new ArgumentException();
            ulong smallCompositionsInBlock = materialCompositionDivisor;
            Debug.Assert(blockArea.valueInMetSq % (smallCompositionsInBlock * rawMaterialArea.valueInMetSq) is 0);
            ulong rawMatTotalAmountInSmallComposition = blockArea.valueInMetSq / (smallCompositionsInBlock * rawMaterialArea.valueInMetSq);

            RawMatAmounts smallComposition = new
            (
                resAmounts: rawMatPropors.Select
                (
                    rawMatAmount => new ResAmount<RawMaterial>
                    (
                        res: rawMatAmount.res,
                        amount: rawMatAmount.amount * rawMatTotalAmountInSmallComposition / totalWeights
                    )
                )
            );
            RawMatAmounts composition = smallComposition * smallCompositionsInBlock;
            Debug.Assert(composition.Area() == blockArea);
            Debug.Assert(composition.All(rawMatAmount => rawMatAmount.amount % materialCompositionDivisor is 0));
            return composition;
        }

        // Want the amount of energy generated from fusion unit area to be proportional to 1 / (ind + 1),
        // i.e. decreasing with ind quite fast
        public static UDouble RawMaterialFusionReactionStrengthCoeff(ulong ind)
            => (ind == maxRawMatInd) ? 0 : (UDouble)0.00000000000000000005 * rawMaterialArea.valueInMetSq / ((RawMaterialMass(ind: ind) - RawMaterialMass(ind: ind + 1)).valueInKg * (ind + 1));

        public static RawMatAmounts CosmicBodyRandomRawMatRatios(RawMatAmounts startingRawMatTargetRatios)
#warning Complete this by making it actually random
            => startingRawMatTargetRatios;

        public static MechComplexity ProductMechComplexity(ProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
#warning Complete this
            => new(complexity: 10);

        private static Propor CombineRawMatProperties(RawMatAmounts rawMatAmounts, Temperature temperature, Func<RawMaterial, Temperature, Propor> rawMatProperty)
            => rawMatAmounts.WeightedAverage
            (
                rawMatAmount =>
                (
                    weight: rawMatAmount.amount,
                    value: rawMatProperty
                    (
                        rawMatAmount.res,
                        temperature
                    )
                )
            );

        private static UDouble CombineRawMatProperties(RawMatAmounts rawMatAmounts, Temperature temperature, Func<RawMaterial, Temperature, UDouble> rawMatProperty)
            => rawMatAmounts.WeightedAverage
            (
                rawMatAmount =>
                (
                    weight: rawMatAmount.amount,
                    value: rawMatProperty
                    (
                        rawMatAmount.res,
                        temperature
                    )
                )
            );

        public static Propor Reflectivity(this RawMatAmounts rawMatAmounts, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: rawMatAmounts,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) =>
                {
                    return Propor.empty;
                    //return (Propor).5;
                    // To look at the graph, paste formula into the link https://www.desmos.com/calculator \frac{1+\tanh\left(\frac{z+1}{5}\right)\ \cdot\sin\left(\left(z+1\right)\left(\frac{x}{500}+1\right)\right)}{2}
                    double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 1));
                    Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                    return (Propor)((1 + scale * wave) / 2);
                }
            );

        // For reflectivity vs reflectance, see https://en.wikipedia.org/wiki/Reflectance#Reflectivity
        // TLDR: reflectivity is the used for thick materials
        public static Propor Reflectivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Reflectivity(rawMatAmounts: roofMatPalette.materialChoices[MaterialPurpose.roofSurface].RawMatComposition, temperature: temperature);

        public static Propor Emissivity(this RawMatAmounts rawMatAmounts, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: rawMatAmounts,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) =>
                {
                    return (Propor).5;
                    // The difference from Reflectivity is + 2 part in sin
                    double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 2));
                    Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                    return (Propor)((1 + scale * wave) / 2);
                }
            );

        public static Propor Emissivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Emissivity
            (
                rawMatAmounts: roofMatPalette.materialChoices[MaterialPurpose.roofSurface].RawMatComposition,
                temperature:temperature
            );

        private static Propor RawMatStartingStrength(ulong ind)
            => (Propor)((UDouble)ind / maxRawMatInd);


        private static (Temperature temperature, Propor strength) RawMatMaxStrength(ulong ind)
            => 
            (
                temperature: Temperature.CreateFromK(valueInK: 100 + ind * 400),
                strength: Propor.full
            );

        /// <summary>
        /// I.e. temperature after which raw material strength will be (close to) zero
        /// </summary>
        private static Temperature RawMatMeltingPoint(ulong ind)
            => Temperature.CreateFromK(valueInK: 2 * RawMatMaxStrength(ind: ind).temperature.valueInK);

        /// <summary>
        /// Currently piecewise linear
        /// </summary>
        private static Propor RawMatStrength(ulong ind, Temperature temperature)
        {
            var (maxStrengthTemper, maxStrength) = RawMatMaxStrength(ind: ind);
            if (temperature <= maxStrengthTemper)
                return Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: temperature.valueInK, start: 0, stop: maxStrengthTemper.valueInK),
                    start: RawMatStartingStrength(ind: ind),
                    stop: maxStrength
                );
            var meltingPoint = RawMatMeltingPoint(ind: ind);
            if (temperature <= meltingPoint)
                return Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: temperature.valueInK, start: maxStrengthTemper.valueInK, stop: meltingPoint.valueInK),
                    start: maxStrength,
                    stop: Propor.empty
                );
            return Propor.empty;
        }

        private static Propor Strength(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatStrength(ind: rawMat.Ind, temperature: temperature)
            );

        // Only public to be testable
        public static (Temperature temperature, Propor resistivity) RawMatResistivityMin(ulong ind)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 200 + ind * 100),
                // basically further raw materials will have more extreme minimums
                resistivity: Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: ind, start: 0, stop: maxRawMatInd),
                    start: RawMatResistivityMid(ind: ind),
                    stop: Propor.empty
                )
            );

        // Only public to be testable
        public static Propor RawMatResistivityMid(ulong ind)
            => (Propor)((2.0 + (ind % 2)) / 5);

        // Only public to be testable
        public static (Temperature temperature, Propor resistivity) RawMatResistivityMax(ulong ind)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 50 + ind * 100),
                // basically further raw materials will have more extreme maximums
                resistivity: Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: ind, start: 0, stop: maxRawMatInd),
                    start: RawMatResistivityMid(ind: ind),
                    stop: Propor.full
                )
            );

        public static Propor RawMatResistivity(ulong ind, Temperature temperature)
        {
            var midResistivity = RawMatResistivityMid(ind: ind);
            return (Propor)((double)midResistivity + Bump(RawMatResistivityMin(ind: ind)) + Bump(RawMatResistivityMax(ind: ind)));

            double Bump((Temperature temperature, Propor resistivity) resistPoint)
            {
                double scaledTemperDiff = ((double)temperature.valueInK - resistPoint.temperature.valueInK) / 50;
                return ((double)resistPoint.resistivity - (double)midResistivity) / (1 + scaledTemperDiff * scaledTemperDiff);
            }
        }

        /// <summary>
        /// Must be from 0 to 1
        /// </summary>
        private static Propor Resistivity(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatResistivity(ind: rawMat.Ind, temperature: temperature)
            );

        private static UDouble BaseElectricalEnergyPerUnitAreaPhys(MaterialPalette electronicsMatPalette, Temperature temperature)
            => (UDouble)0.1 * Resistivity
            (
                material: electronicsMatPalette.materialChoices[MaterialPurpose.electricalConductor],
                temperature: temperature
            );

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, SurfaceGravity gravity, Temperature temperature, UDouble relevantMassPUBA)
#warning make this depend on complexity of production
            => 10 /* amount of useful work. */
                * (1 + electronicsProporInBuilding * BaseElectricalEnergyPerUnitAreaPhys(electronicsMatPalette: electronicsMatPalette, temperature: temperature))
                * (1 + (UDouble)0.1 * gravity.valueInMetPerSeqSq * relevantMassPUBA);

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        public static Propor Throughput(MaterialPalette materialPalette, Temperature temperature)
            => materialPalette.productClass.SwitchExpression
            (
                mechanical: () => Strength(material: materialPalette.materialChoices[MaterialPurpose.mechanical], temperature: temperature),
                electronics: () => Propor.CreateByClamp
                (
                    value: (double)Resistivity
                    (
                        material: materialPalette.materialChoices[MaterialPurpose.electricalInsulator],
                        temperature: temperature
                    ) - (double)Resistivity
                    (
                        material: materialPalette.materialChoices[MaterialPurpose.electricalConductor],
                        temperature: temperature
                    )
                ),
                roof: () => (Propor).5
            );

        public static Propor NeededElectricity(MaterialPalette materialPalette, SurfaceGravity gravity)
            => materialPalette.productClass.SwitchExpression
            (
                mechanical: () => (Propor).5,
                electronics: () => (Propor)1,
                roof: () => (Propor).2
            );

        private const double statsPowerMeanExponent = 0;

        /// <summary>
        /// Throughput from possibly not all mat palette choices
        /// </summary>
        public static Propor TentativeThroughput(Temperature temperature, Propor chosenTotalPropor, EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices,
            EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
            => TentativeStats(input: temperature, matPaletteStats: Throughput, chosenTotalPropor: chosenTotalPropor, matPaletteChoices: matPaletteChoices, buildingProdClassPropors: buildingProdClassPropors);

        /// <summary>
        /// Needed electricity from possibly not all mat palette choices
        /// </summary>
        public static Propor TentativeNeededElectricity(SurfaceGravity gravity, Propor chosenTotalPropor, EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices,
            EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
            => TentativeStats(input: gravity, matPaletteStats: NeededElectricity, chosenTotalPropor: chosenTotalPropor, matPaletteChoices: matPaletteChoices, buildingProdClassPropors: buildingProdClassPropors);

        private static Propor TentativeStats<TInput>(TInput input, Func<MaterialPalette, TInput, Propor> matPaletteStats, Propor chosenTotalPropor,
            EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices, EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
        {
            Debug.Assert(MyMathHelper.AreClose((UDouble)chosenTotalPropor, matPaletteChoices.Keys.Sum(prodClass => (UDouble)buildingProdClassPropors[prodClass])));
            return Propor.PowerMean
            (
                args: matPaletteChoices.Select
                (
                    matPaletteChoice =>
                    (
                        weight: (Propor)((UDouble)buildingProdClassPropors[matPaletteChoice.Key] / (UDouble)chosenTotalPropor),
                        value: matPaletteStats(matPaletteChoice.Value, input)
                    )
                ),
                exponent: statsPowerMeanExponent
            );
        }

        private static readonly UDouble neededElectricityFactor = (UDouble)0.0000001;

        /// <summary>
        /// The number of supplied buildings (assuming that the power plant has throughput of .5, the buildings have neededEnergy .5,
        /// and power plant and each building have the same building area).
        /// </summary>
        private static readonly UDouble powerPlantSuppliedBuildings = 30;

        private static UDouble CurReqWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, AreaDouble buildingArea)
            => buildingArea.valueInMetSq * TentativeNeededElectricity
            (
                gravity: gravity,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * neededElectricityFactor;

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static MechProdStats CurMechProdStats(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, BuildingCostPropors buildingCostPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, Temperature temperature, AreaDouble buildingArea, Mass productionMass)
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    buildingArea: buildingArea
                ),
                ProducedAreaPerSec: buildingArea * TentativeThroughput
                (
                    temperature: temperature,
                    chosenTotalPropor: Propor.full,
                    matPaletteChoices: buildingMatPaletteChoices.Choices,
                    buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
                ) * (UDouble)0.01
            );

        /// <summary>
        /// Note that this returns max production params as it doesn't know how much radiant energy is available to be transformed
        /// </summary>
        public static PowerPlantProdStats CurPowerPlantProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            SurfaceGravity gravity, Temperature temperature, AreaDouble buildingArea)
#warning Complete this
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    buildingArea: buildingArea
                ),
                ProdWatts: buildingArea.valueInMetSq * TentativeThroughput
                (
                    temperature: temperature,
                    chosenTotalPropor: Propor.full,
                    matPaletteChoices: buildingMatPaletteChoices.Choices,
                    buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
                ) * (neededElectricityFactor * powerPlantSuppliedBuildings)
            );

        public static MechProdStats CurConstrStats(AllResAmounts buildingCost, SurfaceGravity gravity, Temperature temperature, ulong worldSecondsInGameSecond)
        {
            var buildingComponentsArea = buildingCost.Area();
#warning Complete this
            return new
            (
                ReqWatts: buildingComponentsArea.valueInMetSq * neededElectricityFactor,
                // Means that the building will complete in 10 real world seconds
                ProducedAreaPerSec: buildingComponentsArea.ToDouble() * ((UDouble)1.0 / (worldSecondsInGameSecond * 10))
            );
        }

        public static ulong MaxAmount(AreaDouble availableArea, AreaInt itemArea)
            => (ulong)availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
