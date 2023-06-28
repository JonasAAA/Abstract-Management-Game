using Game1.Collections;

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

        public static string ConstructionName(string childIndustryName)
            => $"Construction of {childIndustryName}";

        public static string ConstructionComplete(string buildingName)
            => $"Building {buildingName} is complete!";

        public static Result<TOk, TextErrors> ConvertMissingMatPurpsIntoError<TOk>(this Result<TOk, EfficientReadOnlyHashSet<IMaterialPurpose>> result)
            => throw new NotImplementedException();

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
