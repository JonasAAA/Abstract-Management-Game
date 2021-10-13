using Game1.UI;
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
        //private PlayState playState;
        
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
                contentManager: Content,
                graphicsDevice: GraphicsDevice,
                spriteBatch: new(GraphicsDevice)
            );

            //LightManager.Initialize();

            var curGraph = WorldManager.InitializeNew();
            ActiveUI.Initialize(curGraph: curGraph);
            //playState = new();
        }

        protected override void Update(GameTime gameTime)
        {
            exitButton.Update();

            //MyMouse.Update();

            TimeSpan elapsed = gameTime.ElapsedGameTime;

            WorldManager.Current.Update(elapsed: elapsed);

            ActiveUI.Update(elapsed: elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            WorldManager.Current.Draw();

            ActiveUI.DrawHUD();

            base.Draw(gameTime);
        }
    }
}
