namespace Game1.UI
{
    [Serializable]
    public class UIConfig
    {
        public readonly uint standardScreenHeight;
        public readonly UDouble rectOutlineWidth;
        public readonly UDouble letterHeight;
        public readonly Color mouseOnColor;
        public readonly Color defaultUIBackgroundColor;
        // To be used when it should be set to something else before being drawn
        public readonly Color crapColor;
        public readonly Color defaultTextColor;
        public readonly Color defaultAlmostWhiteColor;
        public readonly Color defaultButtonColor;
        public readonly Color defaultSelectedButtonColor;
        public readonly Color defaultDeselectedButtonColor;
        
        public UIConfig()
        {
            standardScreenHeight = 1080;
            rectOutlineWidth = 0;
            letterHeight = 20;
            mouseOnColor = Color.Yellow;
            defaultUIBackgroundColor = Color.White;
            crapColor = Color.Pink;
            defaultTextColor = Color.Black;
            defaultAlmostWhiteColor = Color.Aqua;
            defaultButtonColor = Color.Aqua;
            defaultDeselectedButtonColor = Color.Gray;
        }
    }
}
