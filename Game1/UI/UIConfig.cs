namespace Game1.UI
{
    [Serializable]
    public class UIConfig
    {
        public readonly int standardScreenHeight;
        public readonly float rectOutlineWidth;
        public readonly float letterHeight;
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
