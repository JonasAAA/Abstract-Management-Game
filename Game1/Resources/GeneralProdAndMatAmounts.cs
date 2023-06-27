using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct GeneralProdAndMatAmounts
    {
        public readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts;
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToTargetAreas;
        public readonly AreaInt targetArea;
        public readonly MechComplexity complexity;
        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> materialTargetAreas;
        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> materialPropors;

        public GeneralProdAndMatAmounts(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts, EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> ingredMatPurposeToTargetAreas)
        {
            this.ingredProdToAmounts = ingredProdToAmounts;
            this.ingredMatPurposeToTargetAreas = ingredMatPurposeToTargetAreas;
            materialTargetAreas = IMaterialPurpose.all.ToEfficientReadOnlyDict
            (
                elementSelector: materialPurpose => ingredProdToAmounts.Sum
                (
                    prodParamsAndAmount => prodParamsAndAmount.prodParams.MaterialTargetAreas.GetValueOrDefault(key: materialPurpose) * prodParamsAndAmount.amount
                ) + ingredMatPurposeToTargetAreas.GetValueOrDefault(key: materialPurpose)
            );
            targetArea = materialTargetAreas.Values.Sum();
            complexity = ResAndIndustryAlgos.Complexity(ingredProdToAmounts: ingredProdToAmounts, ingredMatPurposeToTargetAreas: ingredMatPurposeToTargetAreas);
            // Needed to satisfy compiler
            AreaInt targetAreaCopy = targetArea;
            materialPropors = materialTargetAreas.Select
            (
                matPurpAndArea =>
                (
                    materialPurpose: matPurpAndArea.Key,
                    propor: Propor.Create(part: matPurpAndArea.Value.valueInMetSq, targetAreaCopy.valueInMetSq)!.Value
                )
            ).ToEfficientReadOnlyDict
            (
                keySelector: matPurpAndArea => matPurpAndArea.materialPurpose,
                elementSelector: matPurposeAndArea => matPurposeAndArea.propor
            );
        }
    }
}
