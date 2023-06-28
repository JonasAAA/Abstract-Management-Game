using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct GeneralProdAndMatAmounts
    {
        public readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts;
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToUsefulAreas;
        public readonly AreaInt usefulArea;
        public readonly MechComplexity complexity;
        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> materialUsefulAreas;
        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> materialPropors;

        public GeneralProdAndMatAmounts(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToUsefulAreas)
        {
            this.ingredProdToAmounts = ingredProdToAmounts;
            this.ingredMatPurposeToUsefulAreas = ingredMatPurposeToUsefulAreas;
            materialUsefulAreas = IMaterialPurpose.all.ToEfficientReadOnlyDict
            (
                elementSelector: materialPurpose => ingredProdToAmounts.Sum
                (
                    prodParamsAndAmount => prodParamsAndAmount.prodParams.MaterialUsefulAreas.GetValueOrDefault(key: materialPurpose) * prodParamsAndAmount.amount
                ) + ingredMatPurposeToUsefulAreas.GetValueOrDefault(key: materialPurpose)
            );
            usefulArea = materialUsefulAreas.Values.Sum();
            complexity = ResAndIndustryAlgos.Complexity(ingredProdToAmounts: ingredProdToAmounts, ingredMatPurposeToUsefulAreas: ingredMatPurposeToUsefulAreas);
            // Needed to satisfy compiler
            AreaInt usefulAreaCopy = usefulArea;
            materialPropors = materialUsefulAreas.Select
            (
                matPurpAndArea =>
                (
                    materialPurpose: matPurpAndArea.Key,
                    propor: Propor.Create(part: matPurpAndArea.Value.valueInMetSq, usefulAreaCopy.valueInMetSq)!.Value
                )
            ).ToEfficientReadOnlyDict
            (
                keySelector: matPurpAndArea => matPurpAndArea.materialPurpose,
                elementSelector: matPurposeAndArea => matPurposeAndArea.propor
            );
        }
    }
}
