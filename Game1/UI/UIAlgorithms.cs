using Game1.Collections;
using Game1.Industries;

namespace Game1.UI
{
    public static class UIAlgorithms
    {
        public static string NotEnoughResourcesToStartProduction
            => "not enough resources to start production";

        public static string NoRawMaterialMixToSplit
            => "no raw material mix to split";

        public static string CosmicBodyContainsUnwantedResources
            => "cosmic body contains unwanted resources";

        public static string CosmicBodyIsMinedOut
            => "cosmic body is mined out";

        public static string NoMaterialIsChosen
            => "no material is chosen";

        public static string GlobalBuildTabTooltip
            => "Builings to construct";

        public static string BuildHereTooltip
            => "Build on this cosmic body";

        public static string CancelBuilding
            => "Cancel building";

        public static string ConstructionName(string childIndustryName)
            => $"Construction of {childIndustryName}";

        public static string ConstructionTooltip(Construction.GeneralParams constrGeneralParams)
#warning Complete this by adding info from constrGeneralParams.buildingGeneralParams
            => $"{constrGeneralParams.buildingGeneralParams.Name}\nEnergy priority {constrGeneralParams.energyPriority}\n";

        public static string ChooseMaterialForMaterialPurpose(Material material, IMaterialPurpose materialPurpose)
            => $"Choose {material.Name} for {materialPurpose.Name}";

        public static string StartMaterialChoiceForPurposeTooltip(IMaterialPurpose materialPurpose)
            => $"Choose {materialPurpose.Name} material";

        public static string ConstructionComplete(string buildingName)
            => $"Building {buildingName} is complete!";

        public static Result<TOk, TextErrors> ConvertMissingMatPurpsIntoError<TOk>(this Result<TOk, EfficientReadOnlyHashSet<IMaterialPurpose>> result)
            => result.SwitchExpression<Result<TOk, TextErrors>>
            (
                ok: okValue => new(ok: okValue),
                error: missingMatPurposes => new(errors: new($"The following materials need to be chosen:\n{string.Join("\n", missingMatPurposes.Select(missingMatPurpose => missingMatPurpose.Name))}"))
            );

        public static Color MixColorsAndMakeTransparent(Propor transparency, Color baseColor, Color otherColor, Propor otherColorPropor)
            => MixColors
            (
                baseColor: transparency * baseColor,
                otherColor: transparency * otherColor,
                otherColorPropor: otherColorPropor
            );

        public static Color MixColors(Color baseColor, Color otherColor, Propor otherColorPropor)
            => Color.Lerp(baseColor, otherColor, amount: (float)otherColorPropor);
    }
}
