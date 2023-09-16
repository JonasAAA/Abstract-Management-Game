using Game1.Collections;
using Game1.Industries;
using Game1.Resources;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        public static string RawMaterialName(uint ind)
            => $"Raw material {ind}";

        // Formula is like this for the following reasons:
        // * Want density -> 0 as ind -> inf. In this case, density is approximately 1 / ind
        // * Want each fusion reaction to produce energy (i.e. mass after the reaction to be smaller than before)
        // * Want the proportion of mass transformed to energy from fusion reactions to go to 0 as ind -> inf.
        //   In this case, approximately 1 / (ind + 1)
        // * Want the above relationships to be nice and smooth
        // * Want first materials to not have large mass
        // Paste 1-\frac{\operatorname{round}\left(a\cdot\frac{2^{\operatorname{round}\left(x\right)+1}}{\operatorname{round}\left(x\right)+1}\right)}{2\cdot\operatorname{round}\left(a\cdot\frac{2^{\operatorname{round}\left(x\right)}}{\operatorname{round}\left(x\right)}\right)}
        // In https://www.desmos.com/calculator to see that a=3 gives really nice results for the proportion of mass transformed into energy
        public static Mass RawMaterialMass(uint ind)
            => Mass.CreateFromKg
            (
                valueInKg: MyMathHelper.Round
                (
                    (UDouble)(3 * MyMathHelper.Pow(@base: 2, exponent: ind + 1)) / (ind + 1)
                )
            );

        // As ind increases, the raw materials require less energy to change temperature by one degree
        // As said in https://en.wikipedia.org/wiki/Specific_heat_capacity#Monatomic_gases
        // heat capacity per mole is the same for all monatomic gases
        // That's because the atoms have nowhere else to store energy, other than in kinetic energy (and hence temperature)
        public static HeatCapacity RawMaterialHeatCapacity(uint ind)
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        // Formula is like this so that maximum density is 1 and fusion reactions don't change cosmic body area
        // The non-changing area is nice as only mining and planet enlargement buildings need to change size in this case
        public static Area RawMaterialArea(uint ind)
            => Area.CreateFromMetSq(valueInMetSq: 3 * MyMathHelper.Pow(2, ind + 1));

        public static uint MaxRawMatInd
            => 9;

        /// <summary>
        /// All material useful areas must the this.
        /// Ideally, the areas would be this as well.
        /// Currently the way to achieve such is to never create raw material with index more than 9 (so 10 raw materials in total)
        /// and have raw material area propor weights sum to no more than 10.
        /// COULD mutiply by 100 instead of 8 * 9 * 5 * 7, and force each raw material to specify the exact percentage of material area it should take up.
        /// </summary>
        public static Area MaterialUsefulArea
            => RawMaterialArea(ind: MaxRawMatInd) * 8 * 9 * 5 * 7;

        public static RawMatAmounts CreateMatCompositionFromRawMatPropors(RawMatAmounts rawMatAreaPropors)
        {
            UInt96 totalWeights = rawMatAreaPropors.Sum(resAmount => resAmount.amount);
            if (totalWeights == 0)
                throw new ArgumentException();
            if (totalWeights > 10)
                throw new ArgumentException();
            RawMatAmounts composition = new
            (
                resAmounts: rawMatAreaPropors.Select
                (
                    rawMatAmount => rawMatAmount * (MaterialUsefulArea.valueInMetSq / (totalWeights * rawMatAmount.res.Area.valueInMetSq))
                )
            );
            var bla = composition.Area();
            Debug.Assert(composition.Area() == MaterialUsefulArea);
            return composition;
        }

        // As ind increases, the color becomes more brown
        // Formula is plucked out of thin air
        public static Color RawMaterialColor(uint ind)
            => Color.Lerp(Color.Green, Color.Brown, amount: (float)MyMathHelper.Tanh(ind / 3.0));

        // The bigger the number, the easier this raw material will react with itself
        // Formula is plucked out of thin air
        public static UDouble RawMaterialFusionReactionStrengthCoeff(uint ind)
            => (UDouble)0.000000000000001 * (MaxRawMatInd - ind);

        public static RawMatAmounts CosmicBodyRandomRawMatRatios(RawMatAmounts startingRawMatTargetRatios)
#warning Complete this by making it actually random
            => startingRawMatTargetRatios;

        public static MechComplexity IndustryMechComplexity(EfficientReadOnlyCollection<(Product.Params prodParams, uint amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IProductClass, Propor> productClassPropors)
#warning Complete this
            => new(complexity: 10);

        public static MechComplexity ProductMechComplexity(IProductClass productClass, uint materialPaletteAmount, uint indInClass, EfficientReadOnlyCollection<(Product.Params prodParams, uint amount)> ingredProdToAmounts)
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

        public static UDouble DiskBuildingHeight
#warning Complete this by scaling it appropriately (depending on the map scale) and putting it into config file
            => 1000;

        /// <exception cref="ArgumentException">if buildingMatPaletteChoices doesn't contain all required product classes</exception>
        public static EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> BuildingComponentsToAmountPUBA(
            EfficientReadOnlyCollection<(Product.Params prodParams, uint amount)> buildingComponentPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, Propor buildingComponentsProporOfBuildingArea)
        {
            Area buildingComponentProporsTotalArea = buildingComponentPropors.Sum
            (
                prodParamsAndAmount => prodParamsAndAmount.prodParams.usefulArea * prodParamsAndAmount.amount
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

        public static CurProdStats CurConstrStats(AllResAmounts buildingCost, UDouble gravity, Temperature temperature, uint worldSecondsInGameSecond)
        {
            var buildingComponentsUsefulArea = buildingCost.UsefulArea();
#warning Complete this
            return new
            (
                ReqWatts: buildingComponentsUsefulArea.valueInMetSq / 100000,
                // Means that the building will complete in 10 real world seconds
                ProducedAreaPerSec: buildingComponentsUsefulArea.valueInMetSq / (worldSecondsInGameSecond * 10)
            );
        }

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static CurProdStats CurMechProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            UDouble gravity, Temperature temperature, Area buildingArea, Mass productionMass)
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
        public static decimal CurProducedWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            UDouble gravity, Temperature temperature, Area buildingArea, decimal incidentWatts)
#warning Complete this
            => incidentWatts * (UDouble).5;

        private static UDouble RelevantMassPUBA(EfficientReadOnlyDictionary<IProductClass, Propor> buildingProdClassPropors, MaterialPaletteChoices buildingMatPaletteChoices, UDouble productionMassPUBA)
            => throw new NotImplementedException();

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUBA">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the splittingMass of matAmounts and products</param>
        /// <param Name="mechStrength">Mechanical component strength</param>
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

        public static UInt96 MaxAmount(Area availableArea, Area itemArea)
            => availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
