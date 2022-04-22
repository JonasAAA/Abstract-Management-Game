﻿using Game1.Delegates;

namespace Game1.GameStates
{
// This class has almost no static members as it is singleton.
#pragma warning disable CA1822 // Mark members as static
    [Serializable]
    public class PlayState : GameState
    {
        public static readonly PlayState curPlayState = new();

        public event Action OnCreate
        {
            add => WorldManager.OnCreate += value;
            remove => WorldManager.OnCreate -= value;
        }

        private KeyButton? switchToPauseMenuButton;

        private PlayState()
        { }

        public void Initialize(IAction switchToPauseMenu)
            => switchToPauseMenuButton = new
            (
                key: Keys.Escape,
                action: switchToPauseMenu
            );

        public void StartNewGame(GraphicsDevice graphicsDevice)
            => WorldManager.CreateWorldManager(graphicsDevice: graphicsDevice);

        public bool CanContinueGame()
            => WorldManager.Initialized || WorldManager.SaveFileExists;

        public void ContinueGame(GraphicsDevice graphicsDevice)
        {
            if (WorldManager.Initialized)
                return;
            WorldManager.LoadWorldManager(graphicsDevice: graphicsDevice);
        }

        public override void Update(TimeSpan elapsed)
        {
            WorldManager.CurWorldManager.Update(elapsed: elapsed);
            if (switchToPauseMenuButton is null)
                throw new InvalidOperationException($"Must call {nameof(Initialize)} first");
            switchToPauseMenuButton.Update();
        }

        public void SaveGame()
            => WorldManager.CurWorldManager.Save();

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Transparent);
            WorldManager.CurWorldManager.Draw(graphicsDevice: graphicsDevice);
        }
    }
#pragma warning restore CA1822 // Mark members as static
}
