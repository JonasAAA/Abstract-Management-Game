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

        public static string NoResourceIsChosen
            => "no resource is chosen";

        public static string ChangeResSources
            => "Change sources of\nconsumed resources";

        public static string ProducedResourcesDestinations
            => "Produced resources\ndestinations";

        public static string AddOrRemoveResSourceTooltip(IResource res)
            => $"Add or remove source of {res}";

        public static string ToggleSourceTooltip(IResource res, bool add)
            => $"{(add ? "Choose" : "Remove")} this building as source of {res}";

        public static string ToggleSourceButtonName(bool add)
            => $"{(add ? "Choose" : "Remove")} this\nsource";

        public static string AddResSourceForBuildingTooltip(IResource res)
            => $"Add new source of {res}";

        public static string NoResourcesProduced
            => "Produces no\nresources";

        public static string NoSourcesNeeded
            => "Consumes no\nresources";
        
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
                error: missingMatPurposes => new(errors: new(ConvertMissingMatPurpsIntoErrorMessage(missingMatPurposes)))
            );

        public static TOk UnwrapOrThrow<TOk>(this Result<TOk, EfficientReadOnlyHashSet<IMaterialPurpose>> result)
            => result.UnwrapOrThrow
            (
                exception: missingMatChoices => new ArgumentException(ConvertMissingMatPurpsIntoErrorMessage(missingMatChoices))
            );

        private static string ConvertMissingMatPurpsIntoErrorMessage(EfficientReadOnlyHashSet<IMaterialPurpose> missingMatPurposes)
            => $"The following materials need to be chosen:\n{string.Join("\n", missingMatPurposes.Select(missingMatPurpose => missingMatPurpose.Name))}";

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
