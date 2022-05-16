using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;

namespace Game1
{
    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly PlayState playState;
        private GameState gameState;
        private ActionButton continueButton;

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

            playState = PlayState.curPlayState;
            // I know that continueButton and gameState will be initialized in LoadContent and will not be used before then
            continueButton = null!;
            gameState = null!;
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
            continueButton = new
            (
                shape: CreateActionButtonShape(),
                action: () =>
                {
                    playState.ContinueGame();
                    SetGameState(newGameState: playState);
                },
                text: "Continue",
                tooltip: new ImmutableTextTooltip(text: "Continue from last save")
            )
            {
                PersonallyEnabled = playState.CanContinueGame()
            };
            playState.OnCreate += () =>
            {
                continueButton.PersonallyEnabled = playState.CanContinueGame();
                Debug.Assert(continueButton.PersonallyEnabled);
            };
            MenuState mainMenu = new
            (
                actionButtons: new List<ActionButton>()
                {
                    continueButton,
                    CreateActionButton
                    (
                        action: () =>
                        {
                            playState.StartNewGame();
                            SetGameState(newGameState: playState);
                        },
                        text: "New game",
                        tooltipText: "Start new game"
                    ),
                    CreateActionButton
                    (
                        action: Exit,
                        text: "Exit",
                        tooltipText: "Quit to desktop"
                    ),
                }
            );

            SetGameState(newGameState: mainMenu);

            MenuState pauseMenu = new
            (
                actionButtons: new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        action: () => SetGameState(newGameState: playState),
                        text: "Continue",
                        tooltipText: "Continue from last save"
                    ),
                    CreateActionButton
                    (
                        action: () => playState.SaveGame(),
                        text: "Quick save",
                        tooltipText: "Save the game. Will override the last save"
                    ),
                    CreateActionButton
                    (
                        action: () =>
                        {
                            playState.SaveGame();
                            SetGameState(newGameState: mainMenu);
                        },
                        text: "Save and exit",
                        tooltipText: "Save the game and exit. Will override the last save"
                    ),
                }
            );
            playState.Initialize
            (
                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: pauseMenu)
            );

            return;

            NearRectangle CreateActionButtonShape()
                => new MyRectangle(width: buttonWidth, height: buttonHeight);

            ActionButton CreateActionButton(string text, Action action, string tooltipText)
                => new
                (
                    shape: CreateActionButtonShape(),
                    action: action,
                    tooltip: new ImmutableTextTooltip(text: tooltipText),
                    text: text
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
            gameState.Draw();

            base.Draw(gameTime);
        }
    }
}
