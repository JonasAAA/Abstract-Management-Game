namespace Game1
{
    [Serializable]
    public class GameConfig
    {
        public const string gameName = "Abstract Management Game";

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
        public readonly UDouble UILineHeight = 30;
        public readonly ulong pointNumInSmallFunctionGraphs = 100;
        public readonly Propor minFunctionGraphHighlightPropor = (Propor).02;

        private GameConfig()
        { }
    }
}
