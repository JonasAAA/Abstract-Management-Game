using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Game1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly KeyButton exitButton;
        private PlayState playState;
        
        public Game1()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.IsBorderless = true;

            exitButton = new KeyButton
            (
                key: Keys.Escape,
                action: Exit
            );
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
                Content: Content,
                GraphicsDevice: GraphicsDevice,
                spriteBatch: new(GraphicsDevice),
                scrollSpeed: 60,
                resColors: new()
                {
                    [0] = Color.Yellow,
                    [1] = Color.Red,
                    [2] = Color.Blue,
                }
            );

            LightManager.Initialize();

            playState = new();
        }

        protected override void Update(GameTime gameTime)
        {
            exitButton.Update();

            MyMouse.Update();

            playState.Update(elapsed: gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            playState.Draw();

            base.Draw(gameTime);
        }
    }
}
