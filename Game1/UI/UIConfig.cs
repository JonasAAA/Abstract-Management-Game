using Game1.PrimitiveTypeWrappers;

namespace Game1.UI
{
    [Serializable]
    public class UIConfig
    {
        public readonly uint standardScreenHeight;
        public readonly UDouble rectOutlineWidth;
        public readonly UDouble letterHeight;
        public readonly Color mouseOnColor;

        public UIConfig()
        {
            standardScreenHeight = 1080;
            rectOutlineWidth = 0;
            letterHeight = 20;
            mouseOnColor = Color.Yellow;
        }
    }
}
