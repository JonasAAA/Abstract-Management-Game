using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        ///// <summary>
        ///// The returned dictionaries contain ALL material purposes, not just used ones
        ///// </summary>
        ///// <param Name="ingredProdToAmounts"></param>
        ///// <param Name="ingredMatToTargetAreas"></param>
        ///// <returns></returns>
        //public static (EfficientReadOnlyDictionary<IMaterialPurpose, Area> materialTargetAreas, EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors) MaterialTargetAreasAndPropors(
        //    EfficientReadOnlyCollection<(Product.GeneralParams productParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, Area> ingredMatToTargetAreas)
        //{
        //    var materialPurposeToTotalTargetAreas = IMaterialPurpose.all.ToEfficientReadOnlyDict
        //    (
        //        elementSelector: materialPurpose => ingredProdToAmounts.Sum
        //        (
        //            prodParamsAndAmount => prodParamsAndAmount.productParams.materialPurposeToTotalTargetAreas.GetValueOrDefault(key: materialPurpose) * prodParamsAndAmount.amount
        //        ) + ingredMatToTargetAreas.GetValueOrDefault(key: materialPurpose)
        //    );
        //    Area targetArea = materialPurposeToTotalTargetAreas.Values.Sum();
        //    return
        //    (
        //        materialTargetAreas: materialPurposeToTotalTargetAreas,
        //        buildingMatPropors: materialPurposeToTotalTargetAreas.Select
        //        (
        //            matPurpAndArea =>
        //            (
        //                materialPurpose: matPurpAndArea.Key,
        //                propor: Propor.Create(part: matPurpAndArea.Value.valueInMetSq, targetArea.valueInMetSq)!.Value
        //            )
        //        ).ToEfficientReadOnlyDict
        //        (
        //            keySelector: matPurpAndArea => matPurpAndArea.materialPurpose,
        //            elementSelector: matPurposeAndArea => matPurposeAndArea.propor
        //        )
        //    );
        //}

        public static MaterialChoices FilterOutUnneededMaterials(this MaterialChoices materialChoices, GeneralProdAndMatAmounts ingredients)
            => materialChoices.Where(matChoice => ingredients.materialPropors[matChoice.Key] != Propor.empty).ToEfficientReadOnlyDict
            (
                keySelector: matChoice => matChoice.Key,
                elementSelector: matChoice => matChoice.Value
            );

        public static Temperature DestructionPoint(GeneralProdAndMatAmounts ingredients, MaterialChoices materialChoices)
            => materialChoices.Min(materialChoice => materialChoice.Key.DestructionPoint(material: materialChoice.Value));

        public static MechComplexity Complexity(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, Area> ingredMatPurposeToTargetAreas)
            => throw new NotImplementedException();

        // The material melting point generally has no such formula, it depends heavily on what bonds are formed, as discussed in https://qr.ae/pytnoU
        // Also some composites aren't mixed very well, so don't have a single melting point.
        // So the formula below is entirely my creation
        public static Temperature MaterialMeltingPoint(RawMaterialsMix materialComposition)
            => Temperature.CreateFromK
            (
                valueInK: materialComposition.WeightedAverage
                (
                    resAmount => (weight: resAmount.res.Mass.valueInKg * resAmount.amount, value: resAmount.res.MeltingPoint.valueInK)
                )
            );

        public static ulong GatMaterialAmountFromArea(Material material, Area area)
            => throw new NotImplementedException();

        public static UDouble Strength(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        public static Propor Reflectance(this RawMaterialsMix rawMaterial, Temperature temperature)
            => throw new NotImplementedException();

        public static Propor Reflectance(this Material material, Temperature temperature)
            //Reflectance = Propor.Create
            //(
            //    part: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq * resAmount.amount * resAmount.res.Reflectance),
            //    whole: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq * resAmount.amount)
            //)!.Value;
            => throw new NotImplementedException();

        public static Propor Emissivity(this RawMaterialsMix material, Temperature temperature)
            //Emissivity = Propor.Create
            //(
            //    part: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq* resAmount.amount * resAmount.res.Emissivity),
            //    whole: composition.Sum(resAmount => resAmount.res.Area.valueInMetSq* resAmount.amount)
            //)!.Value;
            => throw new NotImplementedException();

        public static Propor Emissivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        public static UDouble Resistivity(this Material material, Temperature temperature)
            => throw new NotImplementedException();

        //public readonly record struct CompProporAndProperty(Propor ComponentPropor, UDouble Property);

        public static UDouble BuildingArea(UDouble surfaceLength)
            => (surfaceLength + BuildingHeight * MyMathHelper.pi) * BuildingHeight;

        public static UDouble BuildingHeight
            => throw new NotImplementedException();

        public static Result<SomeResAmounts<IResource>, TextErrors> BuildingCost(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices, UDouble surfaceLength)
            => throw new NotImplementedException();

        /// <exception cref="ArgumentException">if buildingMatChoices doesn't contain all required materials</exception>
        public static EfficientReadOnlyCollection<(Product prod, UDouble amountPUS)> BuildingComponentsToAmountPUSOrThrow(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, MaterialChoices buildingMatChoices)
            => throw new NotImplementedException();

        public static CurProdStats CurConstrStats(GeneralProdAndMatAmounts buildingCostPropors, UDouble gravity, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static CurProdStats CurMechProdStats(GeneralProdAndMatAmounts buildingCostPropors, MaterialChoices buildingMatChoices,
            UDouble gravity, Temperature temperature, UDouble surfaceLength, Mass productionMass)
        {
            UDouble relevantMassPUS = RelevantMassPUS
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
                productionMassPUS: productionMass.valueInKg / surfaceLength
            );

            UDouble maxMechThroughputPUS = MaxMechThroughputPUS
            (
                buildingMatPropors: buildingCostPropors.materialPropors,
                buildingMatChoices: buildingMatChoices,
                buildingComplexity: buildingCostPropors.complexity,
                gravity: gravity,
                temperature: temperature,
                relevantMassPUS: relevantMassPUS
            );

            UDouble maxElectricalPowerPUS = MaxElectricalPowerPUS
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
                relevantMassPUS: relevantMassPUS
            );

            UDouble
                reqWattsPUS = MyMathHelper.Min(maxElectricalPowerPUS, maxMechThroughputPUS * electricalEnergyPerUnitArea),
                reqWatts = reqWattsPUS * surfaceLength,
                producedAreaPerSec = reqWatts / electricalEnergyPerUnitArea;

            return new
            (
                ReqWatts: reqWatts,
                ProducedAreaPerSec: producedAreaPerSec
            );
        }

        private static UDouble RelevantMassPUS(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, UDouble productionMassPUS)
            => throw new NotImplementedException();

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        /// <param Name="relevantMassPUS">Mass which needs to be moved/rotated. Until structural material purpose is in use, this is all the miningMass of materials and products</param>
        /// <param Name="mechStrength">Mechanical component strength</param>
        private static UDouble MaxMechThroughputPUS(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUS)
        {
            // SHOULD PROBABLY also take into account the complexity of the building
            // This is maximum of restriction from gravity and restriction from mechanical strength compared to total weigtht of things
            throw new NotImplementedException();
        }

        private static UDouble MaxElectricalPowerPUS(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, Temperature temperature)
            => throw new NotImplementedException();

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMatPropors, MaterialChoices buildingMatChoices, MechComplexity buildingComplexity, UDouble gravity, Temperature temperature, UDouble relevantMassPUS)
            => throw new NotImplementedException();

        public static UDouble AreaInProduction(UDouble surfaceLength)
            => throw new NotImplementedException();

        public static ulong AmountInProduction(UDouble areaInProduction, Area itemTargetArea)
            => throw new NotImplementedException();
    }
}
