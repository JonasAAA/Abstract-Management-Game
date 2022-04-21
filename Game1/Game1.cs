using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;

namespace Game1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private PlayState? playState;
        private GameState gameState;
        
        public Game1()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.IsBorderless = true;

            // tries to enable antialiasing. will only work for Monogame 3.8.1 and later
            // this pr fixes the issue https://github.com/MonoGame/MonoGame/pull/7338
            graphics.PreparingDeviceSettings += (sender, e) =>
            {
                graphics.PreferMultiSampling = true;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
            };
        }

        protected override void Initialize()
        {
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;

            static void SetToPreserve(object? sender, PreparingDeviceSettingsEventArgs eventargs)
                => eventargs.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(SetToPreserve);

            graphics.ApplyChanges();

            base.Initialize();
        }

        [Serializable]
        private readonly record struct SetGameStateToPause(Game1 Game, GameState PauseMenu) : IAction
        {
            public void Invoke()
                => Game.SetGameState(newGameState: PauseMenu);
        }

        protected override void LoadContent()
        {
            C.Initialize
            (
                contentManager: Content,
                graphicsDevice: GraphicsDevice
            );

            // TODO: consider moving this to a constants class or similar
            UDouble buttonWidth = 200, buttonHeight = 30;
            MenuState mainMenu = new
            (
                actionButtons: new List<ActionButton>()
                {
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: () =>
                        {
                            if (playState is null)
                                playState = PlayState.LoadGame(graphicsDevice: GraphicsDevice);
                            SetGameState(newGameState: playState);
                        },
                        text: "Continue"
                    ),
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: () =>
                        {
                            playState = PlayState.StartNewGame(graphicsDevice: GraphicsDevice);
                            SetGameState(newGameState: playState);
                        },
                        text: "New game"
                    ),
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: Exit,
                        text: "Exit"
                    ),
                }
            );

            SetGameState(newGameState: mainMenu);

            MenuState pauseMenu = new
            (
                actionButtons: new List<ActionButton>()
                {
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: () => SetGameState(newGameState: playState),
                        text: "Continue"
                    ),
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: () => playState.SaveGame(),
                        text: "Quick save"
                    ),
                    new
                    (
                        shape: new MyRectangle(width: buttonWidth, height: buttonHeight)
                        {
                            Color = Color.White
                        },
                        action: () =>
                        {
                            playState.SaveGame();
                            SetGameState(newGameState: mainMenu);
                        },
                        text: "Save and exit"
                    ),
                }
            );
            PlayState.Initialize
            (
                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: pauseMenu)
            );
        }

        private void SetGameState(GameState newGameState)
        {
            if (gameState is not null)
                gameState.OnLeave();
            gameState = newGameState;
            gameState.OnEnter();
        }

        protected override void Update(GameTime gameTime)
        {
            TimeSpan elapsed = gameTime.ElapsedGameTime;

            gameState.Update(elapsed: elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            gameState.Draw(graphicsDevice: GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
