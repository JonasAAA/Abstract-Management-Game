using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        public static string RawMaterialName(ulong ind)
            => $"Raw material {ind}";

        public static Mass RawMaterialMass(ulong ind)
            => Mass.CreateFromKg(valueInKg: MyMathHelper.Pow(@base: 2, exponent: ind) + 1);

        // As ind increases, the raw materials require less energy to change temperature by one degree
        // As said in https://en.wikipedia.org/wiki/Specific_heat_capacity#Monatomic_gases
        // heat capacity per mole is the same for all monatomic gases
        // That's because the atoms have nowhere else to store energy, other than in kinetic energy (and hence temperature)
        public static HeatCapacity RawMaterialHeatCapacity(ulong ind)
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        // As ind moves in either direction of 2.5, the raw materials become more dense
        // Formula is plucked out of thin air
        public static AreaInt RawMaterialArea(ulong ind)
            => AreaInt.CreateFromMetSq(valueInMetSq: (ind + 1) * (ind + 1));

        // As ind increases, the raw material becomes harder to melt. Raw material 0 is always liquid
        // Formula is plucked out of thin air
        // Could take inspiration from https://en.wikipedia.org/wiki/Melting_point#Predicting_the_melting_point_of_substances_(Lindemann's_criterion)
        public static Temperature RawMaterialMeltingPoint(ulong ind)
            => Temperature.CreateFromK(valueInK: ind * 300);

        // As ind increases, the color becomes more brown
        // Formula is plucked out of thin air
        public static Color RawMaterialColor(ulong ind)
            => Color.Lerp(Color.Green, Color.Brown, amount: (float)MyMathHelper.Tanh(ind / 3.0));

        // The bigger the number, the easier this raw material will react with itself
        // Formula is plucked out of thin air
        public static UDouble RawMaterialFusionReactionStrengthCoeff(ulong ind)
            => (UDouble)0.000000001;

        public static RawMatAmounts CosmicBodyRandomRawMatRatios(RawMatAmounts startingRawMatTargetRatios)
        {
            return startingRawMatTargetRatios;
        }

        public static Temperature DestructionPoint(GeneralProdAndMatAmounts ingredients, MaterialChoices materialChoices)
            => materialChoices.Min(materialChoice => materialChoice.Key.DestructionPoint(material: materialChoice.Value));

        public static MechComplexity Complexity(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToUsefulAreas)
            => throw new NotImplementedException();

        // The material melting point generally has no such formula, it depends heavily on what bonds are formed, as discussed in https://qr.ae/pytnoU
        // Also some composites aren't mixed very well, so don't have a single melting point.
        // So the formula below is entirely my creation
        public static Temperature MaterialMeltingPoint(RawMatAmounts materialComposition)
            => Temperature.CreateFromK
            (
                valueInK: materialComposition.WeightedAverage
                (
                    resAmount => (weight: resAmount.res.Mass.valueInKg * resAmount.amount, value: resAmount.res.MeltingPoint.valueInK)
                )
            );

        public static ulong GatMaterialAmountFromArea(Material material, AreaInt area)
            => throw new NotImplementedException();

        public static Propor Reflectance(this RawMatAmounts rawMaterial, Temperature temperature)
        {
#warning Complete this
            return (Propor).5;
            //=> throw new NotImplementedException();
        }

        public static Propor Reflectance(this Material material, Temperature temperature)
            //Reflectance = Propor.CreateOrThrow
            //(
            //    part: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq * resAmount.amount * resAmount.res.Reflectance),
            //    whole: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq * resAmount.amount)
            //)!.Value;
            => throw new NotImplementedException();

        public static Propor Emissivity(this RawMatAmounts material, Temperature temperature)
        {
#warning Complete this
            return (Propor).5;
            //Emissivity = Propor.CreateOrThrow
            //(
            //    part: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq* resAmount.amount * resAmount.res.Emissivity),
            //    whole: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq* resAmount.amount)
            //)!.Value;
        }

        public static Propor Emissivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        public static UDouble Resistivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        //public readonly record struct CompProporAndProperty(Propor ComponentPropor, UDouble Property);

        public static UDouble DiskBuildingHeight
            => throw new NotImplementedException();

        public static AreaDouble BuildingComponentUsefulArea(AreaDouble buildingArea)
            => throw new NotImplementedException();

        public static Result<AllResAmounts, EfficientReadOnlyHashSet<IMaterialPurpose>> BuildingCost(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices, AreaDouble buildingArea)
            => throw new NotImplementedException();

        /// <exception cref="ArgumentException">if buildingMatChoices doesn't contain all required matAmounts</exception>
        public static Result<EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)>, EfficientReadOnlyHashSet<IMaterialPurpose>> BuildingComponentsToAmountPUBA(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, MaterialChoices buildingMatChoices)
            => throw new NotImplementedException();

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
            => throw new NotImplementedException();

        private static UDouble RelevantMassPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, UDouble productionMassPUBA)
            => throw new NotImplementedException();

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUBA">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the splittingMass of matAmounts and products</param>
        /// <param Name="mechStrength">Mechanical component strength</param>
        private static UDouble MaxMechThroughputPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
        {
            // SHOULD PROBABLY also take into account the complexity of the building
            // This is maximum of restriction from gravity and restriction from mechanical strength compared to total weigtht of things
            throw new NotImplementedException();
        }

        private static UDouble MaxElectricalPowerPUBA(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUBA)
            => throw new NotImplementedException();

        public static AreaDouble AreaInProduction(AreaDouble buildingArea)
            => throw new NotImplementedException();

        public static ulong MaxAmountInProduction(AreaDouble areaInProduction, AreaInt itemUsefulArea)
            => throw new NotImplementedException();
    }
}
