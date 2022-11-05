namespace Game1.UI
{
    [Serializable]
    public sealed class UIConfig
    {
        public readonly uint standardScreenHeight = 1080;
        public readonly UDouble rectOutlineWidth = 0;
        public readonly UDouble letterHeight = 20;
        public readonly Color
            mouseOnColor = Color.Yellow,
            UIBackgroundColor = C.ColorFromRGB(rgb: 0x8193AE),
            textColor = C.ColorFromRGB(rgb: 0x0F1826),
            almostWhiteColor = Color.Aqua,
            buttonColor = Color.Aqua,
            selectedButtonColor = Color.White,
            deselectedButtonColor = Color.Gray,
            tooltipBackgroundColor = Color.LightPink,
            deleteButtonColor = Color.Red;
    }
}
