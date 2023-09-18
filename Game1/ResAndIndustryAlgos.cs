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

        public static Temperature Temperature(HeatEnergy heatEnergy, HeatCapacity heatCapacity)
            => PrimitiveTypeWrappers.Temperature.CreateFromK(valueInK: temperatureScaling * (UDouble)heatEnergy.ValueInJ() / heatCapacity.valueInJPerK);

        public static HeatEnergy HeatEnergyFromTemperature(Temperature temperature, HeatCapacity heatCapacity)
            => HeatEnergy.CreateFromJoules(valueInJ: MyMathHelper.Round(heatCapacity.valueInJPerK * temperature.valueInK / temperatureScaling));

        public static RawMatAmounts CreateRawMatCompositionFromRawMatPropors(RawMatAmounts rawMatPropors)
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
                resAmounts: Algorithms.Split
                (
                    weights: rawMatPropors.ToEfficientReadOnlyDict
                    (
                        keySelector: rawMatAmount => rawMatAmount.res,
                        elementSelector: rawMatAmount => rawMatAmount.amount
                    ),
                    totalAmount: rawMatTotalAmountInSmallComposition
                ).Select(resAndAmount => new ResAmount<RawMaterial>(res: resAndAmount.owner, resAndAmount.amount))
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

        public static MechComplexity IndustryMechComplexity(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IProductClass, Propor> productClassPropors)
#warning Complete this
            => new(complexity: 10);

        public static MechComplexity ProductMechComplexity(IProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
#warning Complete this
            => new(complexity: 10);

        public static Propor Reflectivity(this RawMatAmounts rawMatAmounts, Temperature temperature)
        {
            return rawMatAmounts.WeightedAverage
            (
                rawMatAmount => (weight: rawMatAmount.amount, value: Reflectance(rawMatAmount.res))
            );

            Propor Reflectance(RawMaterial rawMat)
            {
                // To look at the graph, paste formula into the link https://www.desmos.com/calculator \frac{1+\tanh\left(\frac{z+1}{5}\right)\ \cdot\sin\left(\left(z+1\right)\left(\frac{x}{500}+1\right)\right)}{2}
                double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 1));
                Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                return (Propor)((1 + scale * wave) / 2);
            }
        }

        // For reflectivity vs reflectance, see https://en.wikipedia.org/wiki/Reflectance#Reflectivity
        // TLDR: reflectivity is the used for thick materials
        public static Propor Reflectivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Reflectivity(rawMatAmounts: roofMatPalette.materialChoices[IMaterialPurpose.roofSurface].RawMatComposition, temperature: temperature);

        public static Propor Emissivity(this RawMatAmounts rawMatAmounts, Temperature temperature)
        {
            return rawMatAmounts.WeightedAverage
            (
                rawMatAmount => (weight: rawMatAmount.amount, value: Emissivity(rawMatAmount.res))
            );

            Propor Emissivity(RawMaterial rawMat)
            {
                // The difference from Reflectivity is + 2 part in sin
                double wave = MyMathHelper.Sin((rawMat.Ind + 1) * (temperature.valueInK / 500 + 2));
                Propor scale = MyMathHelper.Tanh((rawMat.Ind + 1) / 5);

                return (Propor)((1 + scale * wave) / 2);
            }
        }

        public static Propor Emissivity(this MaterialPalette roofMatPalette, Temperature temperature)
            => Emissivity
            (
                rawMatAmounts: roofMatPalette.materialChoices[IMaterialPurpose.roofSurface].RawMatComposition,
                temperature:temperature
            );

        public static UDouble Resistivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        /// <exception cref="ArgumentException">if buildingMatPaletteChoices doesn't contain all required product classes</exception>
        public static EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> BuildingComponentsToAmountPUBA(
            EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, Propor buildingComponentsProporOfBuildingArea)
        {
            AreaInt buildingComponentProporsTotalArea = buildingComponentPropors.Sum
            (
                prodParamsAndAmount => prodParamsAndAmount.prodParams.area * prodParamsAndAmount.amount
            );
            return buildingComponentPropors.Select
            (
                prodParamsAndAmount =>
                (
                    prod: prodParamsAndAmount.prodParams.GetProduct
                    (
                        materialPalette: buildingMatPaletteChoices[prodParamsAndAmount.prodParams.productClass]
                    ),
                    // This is productAreaPUBA / prodAmount. prodAmount cancelled out.
                    amountPUBA: buildingComponentsProporOfBuildingArea * prodParamsAndAmount.amount / buildingComponentProporsTotalArea.valueInMetSq
                )
            ).ToEfficientReadOnlyCollection();
        }

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
        public static CurProdStats CurMechProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            UDouble gravity, Temperature temperature, AreaDouble buildingArea, Mass productionMass)
#warning Either this or the one that uses it should probably take into account worldSecondsInGameSecond. Probably would like to have separate configurable physics and gameplay speed multipliers
        {
            UDouble relevantMassPUBA = RelevantMassPUBA
            (
                buildingProdClassPropors: buildingCostPropors.productClassPropors,
                buildingMatPaletteChoices: buildingMatPaletteChoices,
                productionMassPUBA: productionMass.valueInKg / buildingArea.valueInMetSq
            );

            UDouble maxMechThroughputPUBA = MaxMechThroughputPUBA
            (
                mechanicalProporInBuilding: buildingCostPropors.productClassPropors[IProductClass.mechanical],
                mechanicalMatPalette: buildingMatPaletteChoices[IProductClass.mechanical],
                buildingComplexity: buildingCostPropors.complexity,
                gravity: gravity,
                temperature: temperature,
                relevantMassPUBA: relevantMassPUBA
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
                buildingComplexity: buildingCostPropors.complexity,
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

        private static UDouble RelevantMassPUBA(EfficientReadOnlyDictionary<IProductClass, Propor> buildingProdClassPropors, MaterialPaletteChoices buildingMatPaletteChoices, UDouble productionMassPUBA)
            => throw new NotImplementedException();

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUBA">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the splittingMass of matAmounts and products</param>
        private static UDouble MaxMechThroughputPUBA(Propor mechanicalProporInBuilding, MaterialPalette mechanicalMatPalette, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            // SHOULD PROBABLY also take into account the complexity of the building
            // This is maximum of restriction from gravity and restriction from mechanical strength compared to total weigtht of things
            => throw new NotImplementedException();

        private static UDouble MaxElectricalPowerPUBA(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            => throw new NotImplementedException();

        public static ulong MaxAmount(AreaDouble availableArea, AreaInt itemArea)
            => (ulong)availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
