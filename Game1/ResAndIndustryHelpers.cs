using Game1.Collections;
using static Game1.WorldManager;

namespace Game1
{
    public static class ResAndIndustryHelpers
    {
        public static Color Color(this RawMatAmounts rawMatAmounts)
        {
            Vector3 colorSum = Vector3.Zero;
            foreach (var (rawMaterial, amount) in rawMatAmounts)
                colorSum += rawMaterial.Area.valueInMetSq * amount * rawMaterial.Color.ToVector3();
            float weightSum = rawMatAmounts.Area().valueInMetSq;
            return new Color(colorSum / weightSum);
        }

        public static AreaDouble ToDouble(this AreaInt area)
            => AreaDouble.CreateFromMetSq(valueInMetSq: area.valueInMetSq);

        public static AreaInt RoundDown(this AreaDouble area)
            => AreaInt.CreateFromMetSq(valueInMetSq: (ulong)area.valueInMetSq);

        public static bool DictEquals<TKey, TValue>(this EfficientReadOnlyDictionary<TKey, TValue> dict, EfficientReadOnlyDictionary<TKey, TValue> otherDict)
            where TKey : notnull
            where TValue : class
        {
            if (dict.Count != otherDict.Count)
                return false;
            foreach (var (key, value) in dict)
                if (!otherDict.TryGetValue(key, out var otherValue) || value != otherValue)
                    return false;
            return true;
        }

        public static TEnergy CurEnergy<TEnergy>(UDouble watts, Propor proporUtilized, TimeSpan elapsed)
            where TEnergy : struct, IUnconstrainedEnergy<TEnergy>
            => IUnconstrainedEnergy<TEnergy>.CreateFromJoules
            (
                valueInJ: MyMathHelper.RoundNonneg((decimal)watts * (decimal)proporUtilized * (decimal)elapsed.TotalSeconds)
            );

        public static Propor WorkingPropor(Propor proporUtilized, ElectricalEnergy allocatedEnergy, ElectricalEnergy reqEnergy)
        {
            if (reqEnergy.IsZero)
                return proporUtilized;
            return proporUtilized * Propor.Create(part: allocatedEnergy.ValueInJ, whole: reqEnergy.ValueInJ)!.Value;
        }

        public static Propor UpdateDonePropor(this Propor donePropor, Propor workingPropor, AreaDouble producedAreaPerSec, TimeSpan elapsed, AreaInt areaInProduction)
        {
            AreaDouble areaProduced = workingPropor * (UDouble)elapsed.TotalSeconds * producedAreaPerSec;
            return Propor.CreateByClamp((UDouble)donePropor + areaProduced.valueInMetSq / areaInProduction.valueInMetSq);
        }

        /// <exception cref="ArgumentException">if buildingMatPaletteChoices doesn't contain all required product classes</exception>
        public static BuildingComponentsToAmountPUBA BuildingComponentsToAmountPUBA(
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

        /// <summary>
        /// The only difference from CurNeededBuildingComponents is rounding down instead of up
        /// </summary>
        public static AllResAmounts MaxBuildingComponentsInArea(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, AreaDouble area, Propor buildingComponentsProporOfBuildingArea)
            => new
            (
                buildingComponentsToAmountPUBA.Select
                (
                    prodAndAmountPUBA =>
                    {
                        var bla = new ResAmount<IResource>
                        (
                            res: prodAndAmountPUBA.prod,
                            // 1 is in here as otherwise tiny landfills may be unable to store any building component, and thus be unable to do landfilling
                            amount: MyMathHelper.Max(1, (ulong)(prodAndAmountPUBA.amountPUBA * area.valueInMetSq / (UDouble)buildingComponentsProporOfBuildingArea))
                        );
                        return bla;
                    }
                )
            );

        public static AllResAmounts CurNeededBuildingComponents(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, AreaDouble curBuildingArea)
            => new
            (
                buildingComponentsToAmountPUBA.Select
                (
                    prodAndAmountPUBA => new ResAmount<IResource>
                    (
                        res: prodAndAmountPUBA.prod,
                        amount: MyMathHelper.Ceiling(prodAndAmountPUBA.amountPUBA * curBuildingArea.valueInMetSq)
                    )
                )
            );
    }
}
