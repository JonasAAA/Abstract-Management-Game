using Game1.Collections;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1
{
    public static class ResAndIndustryHelpers
    {
        public static AllResAmounts ToAll(this SomeResAmounts<IResource> resAmounts)
            => new(resAmounts: resAmounts, rawMatsMix: RawMaterialsMix.empty);

        public static AllResAmounts ToAll(this SomeResAmounts<Material> matAmounts)
            => new(resAmounts: matAmounts.Generalize(), rawMatsMix: RawMaterialsMix.empty);

        public static AllResAmounts ToAll(this SomeResAmounts<Product> prodAmounts)
            => new(resAmounts: prodAmounts.Generalize(), rawMatsMix: RawMaterialsMix.empty);

        public static AllResAmounts ToAll(this RawMaterialsMix rawMatsMix)
            => new(resAmounts: SomeResAmounts<IResource>.empty, rawMatsMix: rawMatsMix);

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

        public static SomeResAmounts<IResource> CurNeededBuildingComponents(EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA, AreaDouble curBuildingArea)
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
            var buildingComponentsToRemove = buildingResPile.Amount.resAmounts - CurNeededBuildingComponents(buildingComponentsToAmountPUBA, curBuildingArea);
            if (buildingComponentsToRemove.UsefulArea() >= CurWorldConfig.)
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
