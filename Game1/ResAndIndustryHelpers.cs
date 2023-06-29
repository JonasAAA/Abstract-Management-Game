using Game1.Collections;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1
{
    public static class ResAndIndustryHelpers
    {
        public static AreaInt Area(this SomeResAmounts<RawMaterial> rawMatsMix)
            => rawMatsMix.Sum(resAmount => resAmount.res.Area * resAmount.amount);

        public static Color Color(this SomeResAmounts<RawMaterial> rawMatsMix)
            => throw new NotImplementedException();

        public static AreaDouble ToDouble(this AreaInt area)
            => AreaDouble.CreateFromMetSq(valueInMetSq: area.valueInMetSq);

        public static AreaInt RoundDown(this AreaDouble area)
            => AreaInt.CreateFromMetSq(valueInMetSq: (ulong)area.valueInMetSq);

        public static MaterialChoices FilterOutUnneededMaterials(this MaterialChoices materialChoices, EfficientReadOnlyDictionary<IMaterialPurpose, Propor> materialPropors)
            => materialChoices.Where(matChoice => materialPropors[matChoice.Key] != Propor.empty).ToEfficientReadOnlyDict
            (
                keySelector: matChoice => matChoice.Key,
                elementSelector: matChoice => matChoice.Value
            );

        public static AllResAmounts CurNeededBuildingComponents(EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA, AreaDouble curBuildingArea)
            => new
            (
                buildingComponentsToAmountPUBA.Select
                (
                    prodAndAmountPUBA => new ResAmount<IResource>
                    (
                        prodAndAmountPUBA.prod,
                        MyMathHelper.Ceiling(prodAndAmountPUBA.amountPUBA * curBuildingArea.valueInMetSq)
                    )
                )
            );

        public static void RemoveUnneededBuildingComponents(IIndustryFacingNodeState nodeState, ResPile buildingResPile, EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA, AreaDouble curBuildingArea)
        {
            var buildingComponentsToRemove = buildingResPile.Amount - CurNeededBuildingComponents(buildingComponentsToAmountPUBA, curBuildingArea);
            if (buildingComponentsToRemove.UsefulArea() >= CurWorldConfig.minUsefulBuildingComponentAreaToRemove)
            {
                nodeState.StoredResPile.TransferFrom
                (
                    source: buildingResPile,
                    amount: buildingComponentsToRemove
                );
            }
        }
    }
}
