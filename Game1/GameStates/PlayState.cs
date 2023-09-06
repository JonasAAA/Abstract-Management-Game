using Game1.ContentHelpers;
using Game1.Delegates;
using static Game1.UI.ActiveUIManager;

namespace Game1.GameStates
{
    // This class has almost no static members as it is singleton.
#pragma warning disable CA1822 // Mark members as static
    [Serializable]
    public sealed class PlayState : GameState
    {
        public static PlayState StartGame(IAction switchToPauseMenu, FullValidMapInfo mapInfo)
        {
            WorldManager.CreateWorldManager(mapInfo: mapInfo);
            return new(switchToPauseMenu: switchToPauseMenu);
        }

        public static Result<PlayState, string> ContinueFromSave(IAction switchToPauseMenu, string saveFilePath)
        {
            try
            {
                WorldManager.LoadWorldManager(saveFilePath: saveFilePath);
            }
            catch (Exception exception)
            {
                return new(errors: exception.Message);
            }
            return new(ok: new(switchToPauseMenu: switchToPauseMenu));
        }

        private readonly KeyButton switchToPauseMenuButton;

        private PlayState(IAction switchToPauseMenu)
            => switchToPauseMenuButton = new
            (
                key: Keys.Escape,
                action: switchToPauseMenu
            );

        public sealed override void Update(TimeSpan elapsed)
        {
            WorldManager.CurWorldManager.Update(elapsedGameTime: elapsed);
            switchToPauseMenuButton.Update();
        }

        public void SaveGame(string saveFilePath)
            => WorldManager.CurWorldManager.Save(saveFilePath: saveFilePath);

        public sealed override void Draw()
        {
            C.GraphicsDevice.Clear(colorConfig.cosmosBackgroundColor);
            WorldManager.CurWorldManager.Draw();
        }
    }
#pragma warning restore CA1822 // Mark members as static
}
