using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        public static string RawMaterialName(ulong ind)
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
        public static Mass RawMaterialMass(ulong ind)
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
        public static HeatCapacity RawMaterialHeatCapacity(ulong ind)
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        // Formula is like this so that maximum density is 1 and fusion reactions don't change cosmic body area
        // The non-changing area is nice as only mining and planet enlargement buildings need to change size in this case
        // 
        public static AreaInt RawMaterialArea(ulong ind)
            => AreaInt.CreateFromMetSq(valueInMetSq: 3 * MyMathHelper.Pow(2, ind + 1));

        // As ind increases, the color becomes more brown
        // Formula is plucked out of thin air
        public static Color RawMaterialColor(ulong ind)
            => Color.Lerp(Color.Green, Color.Brown, amount: (float)MyMathHelper.Tanh(ind / 3.0));

        // The bigger the number, the easier this raw material will react with itself
        // Formula is plucked out of thin air
        public static UDouble RawMaterialFusionReactionStrengthCoeff(ulong ind)
            => (UDouble)0.000000000000001;

        public static RawMatAmounts CosmicBodyRandomRawMatRatios(RawMatAmounts startingRawMatTargetRatios)
#warning Complete this by making it actually random
            => startingRawMatTargetRatios;

        public static MechComplexity Complexity(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToUsefulAreas)
#warning Complete this
            => new(complexity: 10);

        public static ulong GatMaterialAmountFromArea(Material material, AreaInt area)
            => MyMathHelper.DivideThenTakeCeiling(dividend: area.valueInMetSq, divisor: material.Area.valueInMetSq);

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
        public static Propor Reflectivity(this Material material, Temperature temperature)
            => Reflectivity(rawMatAmounts: material.RawMatComposition, temperature: temperature);

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

        public static Propor Emissivity(this Material material, Temperature temperature)
            => Emissivity(rawMatAmounts: material.RawMatComposition, temperature:temperature);

        public static UDouble Resistivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        //public readonly record struct CompProporAndProperty(Propor ComponentPropor, UDouble Property);

        public static UDouble DiskBuildingHeight
#warning Complete this by scaling it appropriately (depending on the map scale) and putting it into config file
            => 1000;

        public static Result<AllResAmounts, EfficientReadOnlyHashSet<IMaterialPurpose>> BuildingCost(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices, AreaDouble buildingArea)
            => throw new NotImplementedException();

        /// <exception cref="ArgumentException">if buildingMatChoices doesn't contain all required matAmounts</exception>
        public static Result<EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)>, EfficientReadOnlyHashSet<IMaterialPurpose>> BuildingComponentsToAmountPUBA(
            EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, MaterialChoices buildingMatChoices, Propor buildingComponentsProporOfBuildingArea)
        {
            AreaInt buildingComponentProporsTotalArea = buildingComponentPropors.Sum(prodParamsAndAmount => prodParamsAndAmount.prodParams.usefulArea * prodParamsAndAmount.amount);
            return buildingComponentPropors.SelectMany
            (
                prodParamsAndAmount => prodParamsAndAmount.prodParams.GetProduct(materialChoices: buildingMatChoices).Select
                (
                    func: product =>
                    (
                        prod: product,
                        amountPUBA: buildingComponentsProporOfBuildingArea * prodParamsAndAmount.amount * prodParamsAndAmount.prodParams.usefulArea.valueInMetSq / buildingComponentProporsTotalArea.valueInMetSq
                    )
                )
            ).Select(prodToAmountPUBA => prodToAmountPUBA.ToEfficientReadOnlyCollection());
        }

        public static CurProdStats CurConstrStats(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors, UDouble gravity, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static CurProdStats CurMechProdStats(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices,
            UDouble gravity, Temperature temperature, AreaDouble buildingArea, Mass productionMass)
        {
            UDouble relevantMassPUBA = RelevantMassPUBA
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
                productionMassPUBA: productionMass.valueInKg / buildingArea.valueInMetSq
            );

            UDouble maxMechThroughputPUBA = MaxMechThroughputPUBA
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
                buildingComplexity: buildingCostPropors.complexity,
                gravity: gravity,
                temperature: temperature,
                relevantMassPUBA: relevantMassPUBA
            );

            UDouble maxElectricalPowerPUBA = MaxElectricalPowerPUBA
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
                temperature: temperature
            );

            UDouble electricalEnergyPerUnitArea = ElectricalEnergyPerUnitAreaPhys
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
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
        public static UDouble CurProducedWatts(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices,
            UDouble gravity, Temperature temperature, AreaDouble buildingArea, UDouble incidentWatts)
#warning Complete this
            => incidentWatts * (UDouble).5;

        private static UDouble RelevantMassPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, UDouble productionMassPUBA)
            => throw new NotImplementedException();

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUBA">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the splittingMass of matAmounts and products</param>
        /// <param Name="mechStrength">Mechanical component strength</param>
        private static UDouble MaxMechThroughputPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            // SHOULD PROBABLY also take into account the complexity of the building
            // This is maximum of restriction from gravity and restriction from mechanical strength compared to total weigtht of things
            => throw new NotImplementedException();

        private static UDouble MaxElectricalPowerPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            => throw new NotImplementedException();

        public static ulong MaxAmount(AreaDouble availableArea, AreaInt itemArea)
            => (ulong)availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
