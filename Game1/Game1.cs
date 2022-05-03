using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;

namespace Game1
{
    public class Game1 : Game
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

        [Serializable]
        private readonly record struct ActionButtonParams(string? Text, string? Explanation, Action Action) : ActionButton.IParams
        {
            public Color TextColor
                => Color.Black;

            public void OnClick()
                => Action.Invoke();
        }

        private readonly record struct ActionButtonShapeParams : MyRectangle.IParams
        {
            public Color Color
                => Color.White;
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
                parameters: new ActionButtonParams
                (
                    Text: "Continue",
                    Explanation: null,
                    Action: () =>
                    {
                        playState.ContinueGame();
                        SetGameState(newGameState: playState);
                    }
                )
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
                        text: "New game",
                        explanation: null,
                        action: () =>
                        {
                            playState.StartNewGame();
                            SetGameState(newGameState: playState);
                        }
                    ),
                    CreateActionButton
                    (
                        text: "Exit",
                        explanation: null,
                        action: Exit
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
                        text: "Continue",
                        explanation: null,
                        action: () => SetGameState(newGameState: playState)
                    ),
                    CreateActionButton
                    (
                        text: "Quick save",
                        explanation: null,
                        action: () => playState.SaveGame()
                    ),
                    CreateActionButton
                    (
                        text: "Save and exit",
                        explanation: null,
                        action: () =>
                        {
                            playState.SaveGame();
                            SetGameState(newGameState: mainMenu);
                        }
                    ),
                }
            );
            playState.Initialize
            (
                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: pauseMenu)
            );

            return;

            MyRectangle CreateActionButtonShape()
                => new
                (
                    width: buttonWidth,
                    height: buttonHeight,
                    parameters: new ActionButtonShapeParams()
                );

            ActionButton CreateActionButton(string? text, string? explanation, Action action)
                => new
                (
                    shape: CreateActionButtonShape(),
                    parameters: new ActionButtonParams
                    (
                        Text: text,
                        Explanation: explanation,
                        Action: action
                    )
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
