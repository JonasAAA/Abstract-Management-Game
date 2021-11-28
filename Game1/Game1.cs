using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private PlayState playState;
        private GameState gameState;
        
        public Game1()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.IsBorderless = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;

            static void SetToPreserve(object sender, PreparingDeviceSettingsEventArgs eventargs)
                => eventargs.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(SetToPreserve);

            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            C.Initialize
            (
                contentManager: Content,
                graphicsDevice: GraphicsDevice
            );

            ActiveUIManager.Initialize(graphicsDevice: GraphicsDevice);

            const float buttonWidth = 200, buttonHeight = 30;
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
                switchToPauseMenu: () => SetGameState(newGameState: pauseMenu)
            );

            void SetGameState(GameState newGameState)
            {
                if (gameState is not null)
                    gameState.OnLeave();
                gameState = newGameState;
                gameState.OnEnter();
            }
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
