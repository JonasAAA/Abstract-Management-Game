namespace Game1.UI
{
    [Serializable]
    public sealed class UIConfig
    {
        public readonly uint standardScreenHeight;
        public readonly UDouble rectOutlineWidth;
        public readonly UDouble letterHeight;
        public readonly Color mouseOnColor, UIBackgroundColor, textColor, almostWhiteColor, buttonColor, selectedButtonColor, deselectedButtonColor, tooltipBackgroundColor, deleteButtonColor;
        
        public UIConfig()
        {
            standardScreenHeight = 1080;
            rectOutlineWidth = 0;
            letterHeight = 20;
            mouseOnColor = Color.Yellow;
            UIBackgroundColor = Color.White;
            textColor = Color.Black;
            almostWhiteColor = Color.Aqua;
            buttonColor = Color.Aqua;
            selectedButtonColor = Color.White;
            deselectedButtonColor = Color.Gray;
            tooltipBackgroundColor = Color.LightPink;
            deleteButtonColor = Color.Red;
        }
    }
}
