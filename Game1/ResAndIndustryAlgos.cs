using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        public static string RawMaterialName(ulong ind)
            => $"Raw material {ind}";

        // Want max density to be 1
        public static readonly AreaInt rawMaterialArea = AreaInt.CreateFromMetSq(valueInMetSq: RawMaterialMass(ind: 0).valueInKg);

        // Formula is like this for the following reasons:
        // * Want max mass to be quite small in order to have larger amount of blocks in the world
        // * Want mass to decrease with ind
        public static Mass RawMaterialMass(ulong ind)
            => Mass.CreateFromKg(valueInKg: 1 + maxRawMatInd - ind);

        // As ind increases, the raw materials require less energy to change temperature by one degree
        // As said in https://en.wikipedia.org/wiki/Specific_heat_capacity#Monatomic_gases
        // heat capacity per mole is the same for all monatomic gases
        // That's because the atoms have nowhere else to store energy, other than in kinetic energy (and hence temperature)
        public static HeatCapacity RawMaterialHeatCapacityPerArea(ulong ind)
#warning Make this more interesting?
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        public const ulong energyInJPerKgOfMass = 1;

        public const ulong maxRawMatInd = 9;

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

        // As ind increases, the color becomes more brown
        // Formula is plucked out of thin air
        public static Color RawMaterialColor(ulong ind)
            => Color.Lerp(Color.Green, Color.Brown, amount: (float)MyMathHelper.Tanh(ind / 3.0));

        // Want the amount of energy generated from fusion to be proportional to 1 / (ind + 1),
        // i.e. decreasing with ind quite fast
        public static UDouble RawMaterialFusionReactionStrengthCoeff(ulong ind)
            => (ind == maxRawMatInd) ? 0 : (UDouble)0.0000000000000001 * (RawMaterialMass(ind: ind) - RawMaterialMass(ind: ind + 1)).valueInKg / (ind + 1);

        public static RawMatAmounts CosmicBodyRandomRawMatRatios(RawMatAmounts startingRawMatTargetRatios)
#warning Complete this by making it actually random
            => startingRawMatTargetRatios;

        public static MechComplexity ProductMechComplexity(IProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
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
                    // To look at the graph, paste formula into the link https://www.desmos.com/calculator \frac{1+\tanh\left(\frac{z+1}{5}\right)\ \cdot\sin\left(\left(z+1\right)\left(\frac{x}{500}+1\right)\right)}{2}
                    double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 1));
                    Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                    return (Propor)((1 + scale * wave) / 2);
                }
            );

        // For reflectivity vs reflectance, see https://en.wikipedia.org/wiki/Reflectance#Reflectivity
        // TLDR: reflectivity is the used for thick materials
        public static Propor Reflectivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Reflectivity(rawMatAmounts: roofMatPalette.materialChoices[IMaterialPurpose.roofSurface].RawMatComposition, temperature: temperature);

        public static Propor Emissivity(this RawMatAmounts rawMatAmounts, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: rawMatAmounts,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) =>
                {
                    // The difference from Reflectivity is + 2 part in sin
                    double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 2));
                    Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                    return (Propor)((1 + scale * wave) / 2);
                }
            );

        public static Propor Emissivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Emissivity
            (
                rawMatAmounts: roofMatPalette.materialChoices[IMaterialPurpose.roofSurface].RawMatComposition,
                temperature:temperature
            );

        private static UDouble RawMatStartingStrength(ulong ind)
            => (UDouble)ind / maxRawMatInd;

        "TUNE the numbers of industry production stats so that they are at least somewhat reasonable
        private static (Temperature temperature, UDouble strength) RawMatMaxStrength(ulong ind)
            => 
            (
                temperature: Temperature.CreateFromK(valueInK: 100 + ind * 100),
                strength: 1
            );

        /// <summary>
        /// I.e. temperature after which raw material strength will be (close to) zero
        /// </summary>
        private static Temperature RawMatMeltingPoint(ulong ind)
            => Temperature.CreateFromK(valueInK: 2 * RawMatMaxStrength(ind: ind).temperature.valueInK);

        /// <summary>
        /// Currently piecewise linear
        /// </summary>
        private static UDouble RawMatStrength(ulong ind, Temperature temperature)
        {
            var (maxStrengthTemper, maxStrength) = RawMatMaxStrength(ind: ind);
            if (temperature <= maxStrengthTemper)
                return Algorithms.WeightedAverage
                (
                    a: (weight: temperature.valueInK, value: RawMatStartingStrength(ind: ind)),
                    b: (weight: maxStrengthTemper.valueInK - temperature.valueInK, value: maxStrength)
                );
            var meltingPoint = RawMatMeltingPoint(ind: ind);
            if (temperature <= meltingPoint)
                return Algorithms.WeightedAverage
                (
                    a: (weight: temperature.valueInK - maxStrengthTemper.valueInK, value: maxStrength),
                    b: (weight: meltingPoint.valueInK - temperature.valueInK, value: 0)
                );
            return 0;
        }

        /// <summary>
        /// Must be from 0 to 1
        /// </summary>
        private static UDouble Strength(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatStrength(ind: rawMat.Ind, temperature: temperature)
            );

        private static UDouble RelevantMassPUBA(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, UDouble productionMassPUBA)
            => productionMassPUBA + buildingComponentsToAmountPUBA.Sum
            (
                prodAndAmountPUBA => prodAndAmountPUBA.amountPUBA * prodAndAmountPUBA.prod.ProductClass.ProportionOfMassMovingWithMechanicalProduction * prodAndAmountPUBA.prod.Mass.valueInKg
            );

        private static UDouble MaxBaseMechThroughput(MaterialPalette mechanicalMatPalette, Temperature temperature)
            => (UDouble)0.1 * Strength(material: mechanicalMatPalette.materialChoices[IMaterialPurpose.mechanical], temperature: temperature);

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUBA">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the splittingMass of matAmounts and products</param>
        private static double MaxMechThroughputPUBA(Propor mechanicalProporInBuilding, MaterialPalette mechanicalMatPalette, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            // SHOULD PROBABLY also take into account the complexity of the production, i.e. product complexity, some fixed complexity for mining and such
            // This is maximum of restriction from gravity and restriction from mechanical strength compared to total weigtht of things
            => MyMathHelper.Min
            (
                (double)mechanicalProporInBuilding * MaxBaseMechThroughput(mechanicalMatPalette: mechanicalMatPalette, temperature: temperature) - 0.1 * gravity * relevantMassPUBA,
                0.1 * gravity
            );

        // Only public to be testable
        public static (Temperature temperature, UDouble resistivity) RawMatResistivityMin(ulong ind)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 200 + ind * 100),
                // basically further raw materials will have more extreme minimums
                resistivity: (RawMatResistivityMid(ind: ind) * (maxRawMatInd - ind) + 0 * ind) / maxRawMatInd
            );

        // Only public to be testable
        public static UDouble RawMatResistivityMid(ulong ind)
            => (UDouble)(2 + (ind % 2)) / 5;

        // Only public to be testable
        public static (Temperature temperature, UDouble resistivity) RawMatResistivityMax(ulong ind)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 50 + ind * 100),
                // basically further raw materials will have more extreme maximums
                resistivity: (RawMatResistivityMid(ind: ind) * (maxRawMatInd - ind) + 1 * ind) / maxRawMatInd
            );

        public static UDouble RawMatResistivity(ulong ind, Temperature temperature)
        {
            double midResistivity = RawMatResistivityMid(ind: ind);
            return (UDouble)(midResistivity + Bump(RawMatResistivityMin(ind: ind)) + Bump(RawMatResistivityMax(ind: ind)));

            double Bump((Temperature temperature, UDouble resistivity) resistPoint)
            {
                double scaledTemperDiff = ((double)temperature.valueInK - resistPoint.temperature.valueInK) / 50;
                return ((double)resistPoint.resistivity - midResistivity) / (1 + scaledTemperDiff * scaledTemperDiff);
            }
        }

        /// <summary>
        /// Must be from 0 to 1
        /// </summary>
        private static UDouble Resistivity(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatResistivity(ind: rawMat.Ind, temperature: temperature)
            );

        public static UDouble MaxElectricalPower(MaterialPalette electronicsMatPalette, Temperature temperature)
            => (UDouble)0.1 * UDouble.CreateByCuttingOffNegative
            (
                value: (double)Resistivity
                (
                    material: electronicsMatPalette.materialChoices[IMaterialPurpose.electricalInsulator],
                    temperature: temperature
                ) - (double)Resistivity
                (
                    material: electronicsMatPalette.materialChoices[IMaterialPurpose.electricalConductor],
                    temperature: temperature
                )
            );

        private static UDouble MaxElectricalPowerPUBA(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, Temperature temperature)
            => electronicsProporInBuilding * MaxElectricalPower(electronicsMatPalette: electronicsMatPalette, temperature: temperature);

        private static UDouble BaseElectricalEnergyPerUnitAreaPhys(MaterialPalette electronicsMatPalette, Temperature temperature)
            => (UDouble)0.1 * Resistivity
            (
                material: electronicsMatPalette.materialChoices[IMaterialPurpose.electricalConductor],
                temperature: temperature
            );

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
#warning make this depend on complexity of production
            => 10 /* amount of useful work. */
                * (1 + electronicsProporInBuilding * BaseElectricalEnergyPerUnitAreaPhys(electronicsMatPalette: electronicsMatPalette, temperature: temperature))
                * (1 + (UDouble)0.1 * gravity * relevantMassPUBA);

        public static CurProdStats CurConstrStats(AllResAmounts buildingCost, UDouble gravity, Temperature temperature, ulong worldSecondsInGameSecond)
        {
            var buildingComponentsArea = buildingCost.Area();
#warning Complete this
            return new
            (
                ReqWatts: buildingComponentsArea.valueInMetSq / 1000000000,
                // Means that the building will complete in 10 real world seconds
                ProducedAreaPerSec: buildingComponentsArea.valueInMetSq / (worldSecondsInGameSecond * 10)
            );
        }

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static CurProdStats CurMechProdStats(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, BuildingCostPropors buildingCostPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, UDouble gravity, Temperature temperature, AreaDouble buildingArea, Mass productionMass)
#warning Either this or the one that uses it should probably take into account worldSecondsInGameSecond. Probably would like to have separate configurable physics and gameplay speed multipliers
        {
#warning Implement this properly
            //return new
            //(
            //    ReqWatts: buildingArea.valueInMetSq / 1000000000,
            //    ProducedAreaPerSec: buildingArea.valueInMetSq * WorldManager.CurWorldConfig.productionProporOfBuildingArea / (WorldManager.CurWorldConfig.worldSecondsInGameSecond * 4)
            //);
            UDouble relevantMassPUBA = RelevantMassPUBA
            (
                buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                productionMassPUBA: productionMass.valueInKg / buildingArea.valueInMetSq
            );

            // TODO: could diplay the reason that no production is happening to the player
            var maxMechThroughputPUBA = UDouble.CreateByCuttingOffNegative
            (
                value: MaxMechThroughputPUBA
                (
                    mechanicalProporInBuilding: buildingCostPropors.productClassPropors[IProductClass.mechanical],
                    mechanicalMatPalette: buildingMatPaletteChoices[IProductClass.mechanical],
                    gravity: gravity,
                    temperature: temperature,
                    relevantMassPUBA: relevantMassPUBA
                )
            );

            UDouble maxElectricalPowerPUBA = MaxElectricalPowerPUBA
            (
                electronicsProporInBuilding: buildingCostPropors.productClassPropors[IProductClass.electronics],
                electronicsMatPalette: buildingMatPaletteChoices[IProductClass.electronics],
                temperature: temperature
            );

            UDouble electricalEnergyPerUnitArea = ElectricalEnergyPerUnitAreaPhys
            (
                electronicsProporInBuilding: buildingCostPropors.productClassPropors[IProductClass.electronics],
                electronicsMatPalette: buildingMatPaletteChoices[IProductClass.electronics],
                gravity: gravity,
                temperature: temperature,
                relevantMassPUBA: relevantMassPUBA
            );

            UDouble
                reqWattsPUBA = MyMathHelper.Min(maxElectricalPowerPUBA, maxMechThroughputPUBA * electricalEnergyPerUnitArea),
                reqWatts = reqWattsPUBA * buildingArea.valueInMetSq,
                producedAreaPerSec = reqWatts / electricalEnergyPerUnitArea;

            return new
            (
                ReqWatts: reqWatts,
                ProducedAreaPerSec: producedAreaPerSec
            );
        }

        /// <summary>
        /// To be called by po
        /// </summary>
        public static UDouble CurProducedWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            UDouble gravity, Temperature temperature, AreaDouble buildingArea, UDouble incidentWatts)
#warning Complete this
            => incidentWatts * (UDouble).5;

        public static ulong MaxAmount(AreaDouble availableArea, AreaInt itemArea)
            => (ulong)availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
