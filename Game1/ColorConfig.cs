namespace Game1
{
    [Serializable]
    public sealed class ColorConfig
    {
        public readonly Color
            // world colors
            mapCreationCosmicBodyColor,
            minTemperatureColor,
            maxTemperatureColor,
            cosmosBackgroundColor,
            cheapLinkColor,
            costlyLinkColor,
            houseIndustryColor,
            linkTravellerColor,
            mapCreationSelectedWorldUIElementColor,
            defaultIconBackgroundColor,
            matPaletteNotYetChosenBackgroundColor,
            // material palette colors
            otherMechMatPaletteColor,
            defMechMatPaletteColor,
            defElecMatPaletteColor,
            // industry colors
            miningBuildingColor,
            landfillBuildingColor,
            materialProductionBuildingColor,
            manufacturingBuildingColor,
            storageBuildingColor,
            powerPlantbuildingColor,
            lightRedirectionBuildingColor,

            // UI colors
            mouseOnColor,
            UIBackgroundColor,
            textColor,
            buttonColor,
            buttonTextColor,
            selectedButtonColor,
            deselectedButtonColor,
            tooltipBackgroundColor,
            deleteButtonColor,
            functionGraphLineColor,
            functionGraphBackgroundColor,
            functionGraphHighlightColor,
            shouldBeInvisibleIconBackgroundColor,
            barColor,
            barBackgroundColor;


        // colors are assigned in constructor to allow referencing previously defined colors
        public ColorConfig()
        {
            // world colors
            mapCreationCosmicBodyColor = C.ColorFromRGB(rgb: 0x00754D);
            minTemperatureColor = C.ColorFromRGB(rgb: 0x00476B);
            maxTemperatureColor = C.ColorFromRGB(rgb: 0xFFF07A);
            cosmosBackgroundColor = C.ColorFromRGB(rgb: 0x000E24);
            cheapLinkColor = Color.DarkBlue;
            costlyLinkColor = C.ColorFromRGB(rgb: 0x003654);
            houseIndustryColor = Color.Yellow;
            linkTravellerColor = Color.Black;
            mapCreationSelectedWorldUIElementColor = Color.White;
            defaultIconBackgroundColor = Color.White;
            matPaletteNotYetChosenBackgroundColor = Color.Gray;
            // material palette colors taken from https://tailwindcss.com/docs/customizing-colors
            otherMechMatPaletteColor = C.ColorFromRGB(rgb: 0xfb923c); // Orange-400
            defMechMatPaletteColor = C.ColorFromRGB(rgb: 0xfed7aa); // Orange-200
            defElecMatPaletteColor = C.ColorFromRGB(rgb: 0xd9f99d); // Lime-200
            // industry colors
            miningBuildingColor = C.ColorFromRGB(rgb: 0x1d4ed8); // Blue-700
            landfillBuildingColor = C.ColorFromRGB(rgb: 0x6d28d9); // Violet-700
            materialProductionBuildingColor = C.ColorFromRGB(rgb: 0xa21caf); // Fuchsia-700
            manufacturingBuildingColor = C.ColorFromRGB(rgb: 0xbe123c); // Rose-700
            storageBuildingColor = C.ColorFromRGB(rgb: 0xb45309); // Amber-700
            powerPlantbuildingColor = C.ColorFromRGB(rgb: 0x15803d); // Green-700
            lightRedirectionBuildingColor = C.ColorFromRGB(rgb: 0x0e7490); // Cyan-700

            // UI colors
            mouseOnColor = Color.Yellow;
            UIBackgroundColor = C.ColorFromRGB(rgb: 0x124A5E);
            textColor = C.ColorFromRGB(rgb: 0xADE38F);
            buttonColor = Color.DarkBlue;
            buttonTextColor = textColor;
            selectedButtonColor = buttonColor;
            deselectedButtonColor = buttonColor;
            tooltipBackgroundColor = Color.LightPink;
            deleteButtonColor = Color.Red;
            functionGraphLineColor = textColor;
            functionGraphBackgroundColor = Color.Black;
            functionGraphHighlightColor = Color.White * .5f;
            shouldBeInvisibleIconBackgroundColor = Color.Pink;
            barColor = functionGraphLineColor;
            barBackgroundColor = functionGraphBackgroundColor;
        }
    }
}
