﻿namespace Game1
{
    [Serializable]
    public sealed class ColorConfig
    {
        public readonly Color
            // world colors
            Res0Color,
            Res1Color,
            cosmosBackgroundColor,
            starColor,
            houseIndustryColor,
            linkTravellerColor,
            
            // UI colors
            mouseOnColor,
            UIBackgroundColor,
            textColor,
            almostWhiteColor,
            buttonColor,
            selectedButtonColor,
            deselectedButtonColor,
            tooltipBackgroundColor,
            deleteButtonColor;

        // colors are assigned in constructor to allow referencing previously defined colors
        public ColorConfig()
        {
            // world colors
            Res0Color = C.ColorFromRGB(rgb: 0x00754D);
            Res1Color = C.ColorFromRGB(rgb: 0x00632D);
            cosmosBackgroundColor = C.ColorFromRGB(rgb: 0x000E24);
            starColor = C.ColorFromRGB(rgb: 0xB0FF00);
            houseIndustryColor = Color.Yellow;
            linkTravellerColor = Color.Black;

            // UI colors
            mouseOnColor = Color.Yellow;
            UIBackgroundColor = C.ColorFromRGB(rgb: 0x124A5E);
            textColor = C.ColorFromRGB(rgb: 0xADE38F);
            almostWhiteColor = Color.Aqua;
            buttonColor = Color.Aqua;
            selectedButtonColor = C.ColorFromRGB(rgb: 0x03645);
            deselectedButtonColor = UIBackgroundColor;
            tooltipBackgroundColor = Color.LightPink;
            deleteButtonColor = Color.Red;
        }
    }
}