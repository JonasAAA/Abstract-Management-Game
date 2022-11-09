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
            UIBackgroundColor = C.ColorFromRGB(rgb: 0x124A5E),
            textColor = C.ColorFromRGB(rgb: 0xADE38F),
            almostWhiteColor = Color.Aqua,
            buttonColor = Color.Aqua,
            selectedButtonColor = Color.White,
            deselectedButtonColor = Color.Gray,
            tooltipBackgroundColor = Color.LightPink,
            deleteButtonColor = Color.Red;
    }
}
