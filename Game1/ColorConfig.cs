namespace Game1
{
    [Serializable]
    public sealed class ColorConfig
    {
        public readonly Color
            // world colors
            Res0Color,
            Res1Color,
            cosmosBackgroundColor,
            cheapLinkColor,
            costlyLinkColor,
            houseIndustryColor,
            linkTravellerColor,
            mapCreationSelectedWorldUIElementColor,
            // industry colors
            miningBuildingColor,
            manufacturingBuildingColor,
            materialSplittingBuildingColor,

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
            barColor,
            barBackgroundColor;


        // colors are assigned in constructor to allow referencing previously defined colors
        public ColorConfig()
        {
            // world colors
            Res0Color = C.ColorFromRGB(rgb: 0x00754D);
            Res1Color = C.ColorFromRGB(rgb: 0x00632D);
            cosmosBackgroundColor = C.ColorFromRGB(rgb: 0x000E24);
            cheapLinkColor = Color.DarkBlue;
            costlyLinkColor = C.ColorFromRGB(rgb: 0x003654);
            houseIndustryColor = Color.Yellow;
            linkTravellerColor = Color.Black;
            mapCreationSelectedWorldUIElementColor = Color.White;
            // industry colors
            miningBuildingColor = Color.Brown;
            manufacturingBuildingColor = Color.DarkGray;
            materialSplittingBuildingColor = Color.LightGray;

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
            barColor = functionGraphLineColor;
            barBackgroundColor = functionGraphBackgroundColor;
        }
    }
}
