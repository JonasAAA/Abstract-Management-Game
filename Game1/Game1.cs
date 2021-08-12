using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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

            exitButton = new KeyButton
            (
                key: Keys.Escape,
                action: Exit
            );
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = (int)C.ScreenWidth;
            graphics.PreferredBackBufferHeight = (int)C.ScreenHeight;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            C.Initialize
            (
                scrollSpeed: 1,
                Content: Content,
                spriteBatch: new(GraphicsDevice),
                resColors: new()
                {
                    [0] = Color.Yellow,
                    [1] = Color.Red,
                    [2] = Color.Blue,
                }
            );

            playState = new();
        }

        protected override void Update(GameTime gameTime)
        {
            exitButton.Update();

            MyMouse.Update();

            playState.Update(gameTime: gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            playState.Draw();

            base.Draw(gameTime);
        }
    }
}
