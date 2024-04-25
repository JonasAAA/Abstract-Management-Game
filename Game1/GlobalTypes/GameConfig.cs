namespace Game1.GlobalTypes
{
    [Serializable]
    public class GameConfig
    {
        public const string gameName = "Abstract Management Game";
        // const so that could use this in switch statements/expressions
        public const ulong rawMaterialCount = 6;

        public static readonly GameConfig CurGameConfig = new();

        public readonly UDouble
            screenBoundWidthForMapMoving = 3,
            scrollSpeed = 200,
            linkPixelWidth = 10,
            minPlanetPixelRadius = 10;

        // UI
        public readonly UDouble rectOutlineWidth = 0;
        public readonly UDouble defaultGapBetweenUIElements = 10;
        public readonly UDouble standardUIElementWidth = 100;
        public readonly UDouble wideUIElementWidth = 200;
        public readonly UDouble iconWidth = 32;
        public readonly UDouble iconHeight = 32;
        public readonly UDouble smallIconWidth = 32;
        public readonly UDouble smallIconHeight = 32;
        public readonly UDouble UILineHeight = 32;
        public readonly ulong pointNumInSmallFunctionGraphs = 100;
        public readonly Propor minFunctionGraphHighlightPropor = (Propor).02;

        private GameConfig()
        { }
    }
}
