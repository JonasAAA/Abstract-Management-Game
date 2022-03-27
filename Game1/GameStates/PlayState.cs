using Game1.Delegates;

using static Game1.WorldManager;

namespace Game1.GameStates
{
    [Serializable]
    public class PlayState : GameState
    {
        private static KeyButton switchToPauseMenuButton;

        public static void Initialize(IAction switchToPauseMenu)
            => switchToPauseMenuButton = new
            (
                key: Keys.Escape,
                action: switchToPauseMenu
            );

        public static PlayState StartNewGame(GraphicsDevice graphicsDevice)
        {
            CreateWorldManager(graphicsDevice: graphicsDevice);
            return new PlayState();
        }

        public static PlayState LoadGame(GraphicsDevice graphicsDevice)
        {
            LoadWorldManager(graphicsDevice: graphicsDevice);
            return new PlayState();
        }

        private PlayState()
        { }

        public override void Update(TimeSpan elapsed)
        {
            CurWorldManager.Update(elapsed: elapsed);
            switchToPauseMenuButton.Update();
        }

        public void SaveGame()
            => CurWorldManager.Save();

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Transparent);
            CurWorldManager.Draw(graphicsDevice: graphicsDevice);
        }
    }
}
