using Game1.Collections;
using Game1.Industries;

namespace Game1.UI
{
    public static class UIAlgorithms
    {
        public static string ProductName(string prodParamsName, string paletteName)
            => $"{prodParamsName} ({paletteName})";

        public static string ManufacturingBasicName(string prodParamsName)
            => $"Basic {prodParamsName} manufaturing";

        public static string NoMaterialPaletteChosen
            => "no material palette chosen";

        public static string MatPaletteWithThisNameAlreadyExists
            => "Material palette with this name already exists. Pick a different name.";

        public static string ExactSamePaletteAlreadyExists
            => "Material palette with exact same material choices already exists.";

        public static string NotEnoughResourcesToStartProduction
            => "not enough resources to start production";

        // Output storage is not necessarily exactly full, but it can't accomodate any more outputs.
        public static string OutputStorageFullSoNoProduction
            => "output storage full";

        public static string CosmicBodyIsMinedOut
            => "cosmic body is mined out";

        public static string NoMaterialIsChosen
            => "no material is chosen";

        public static string NoResourceIsChosen
            => "no resource is chosen";

        public static string ChangeResNeighbors(NeighborDir neighborDir)
            => $"Change {(neighborDir is NeighborDir.In ? "sources of\nconsumed" : "destinations of\nproduced")} resources";

        public static string ProducedResourcesDestinations
            => "Produced resources\ndestinations";

        public static string ToggleResNeighborTooltip(NeighborDir neighborDir, IResource res)
            => $"Add or remove {(neighborDir is NeighborDir.In ? "sources" : "destins")} of {res}";

        public static string ToggleResNeighborTooltip(NeighborDir neighborDir, IResource res, bool add)
            => $"{(add ? "Choose" : "Remove")} this building as {(neighborDir is NeighborDir.In ? "source" : "destination")} of {res}";

        public static string ToggleResNeighborButtonName(NeighborDir neighborDir, bool add)
            => $"{(add ? "Choose" : "Remove")} this\n{(neighborDir is NeighborDir.In ? "source" : "destination")}";

        public static string AddResSourceForBuildingTooltip(IResource res)
            => $"Add new source of {res}";

        public static string NothingToConfigure
            => "Nothing to configure";

        public static string NoPossibleNeighbors(NeighborDir neighborDir)
            => $"{(neighborDir is NeighborDir.In ? "Consumes" : "Produces")} no\nresources";

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
            => $"{constrGeneralParams.buildingGeneralParams.Name}\n{constrGeneralParams.energyPriority}\n";

        public static string StartMatPaletteChoiceForProductClassTooltip(ProductClass productClass)
            => $"Choose {productClass} material palette";

        public static string ChooseMatPaletteForProductClass(MaterialPalette materialPalette, ProductClass productClass)
            => $"Choose {materialPalette} for {productClass}";

        public static string StartMaterialChoice
            => "Choose material";

        public static string ChooseMaterial(Material material)
            => $"Choose {material}";

        public static string StartResourceChoiceTooltip
            => "Choose resource";

        public static string ChooseResource(IResource resource)
            => $"Choose {resource}";

        public static string ConstructionComplete(string buildingName)
            => $"Building {buildingName} is complete!";

        public static Result<TOk, TextErrors> ConvertMissingMatPurpsIntoError<TOk>(this Result<TOk, EfficientReadOnlyHashSet<MaterialPurpose>> result)
            => result.SwitchExpression<Result<TOk, TextErrors>>
            (
                ok: okValue => new(ok: okValue),
                error: missingMatPurposes => new(errors: new(ConvertMissingMatPurpsIntoErrorMessage(missingMatPurposes)))
            );

        public static TOk UnwrapOrThrow<TOk>(this Result<TOk, EfficientReadOnlyHashSet<MaterialPurpose>> result)
            => result.UnwrapOrThrow
            (
                exception: missingMatPaletteChoices => new ArgumentException(ConvertMissingMatPurpsIntoErrorMessage(missingMatPaletteChoices))
            );

        public static TOk UnwrapOrThrow<TOk>(this Result<TOk, TextErrors> result)
            => result.UnwrapOrThrow
            (
                exception: errors => new ArgumentException($"Error(s) occured:\n{string.Join('\n', errors)}")
            );

        private static string ConvertMissingMatPurpsIntoErrorMessage(EfficientReadOnlyHashSet<MaterialPurpose> missingMatPurposes)
            => $"The following materials need to be chosen:\n{string.Join('\n', missingMatPurposes)}";

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
