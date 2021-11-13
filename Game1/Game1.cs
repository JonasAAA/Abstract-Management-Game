using Game1.Events;
using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static Game1.UI.ActiveUIManager;

namespace Game1
{
    public class Game1 : Game
    {
        private record NewGameButtonClickedListener(Button NewGameButton, GraphicsDevice GraphicsDevice) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => WorldManager.CreateWorldUIManager(graphicsDevice: GraphicsDevice);
        }

        private readonly GraphicsDeviceManager graphics;
        private readonly KeyButton exitButton;
        
        public Game1()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.IsBorderless = true;

            exitButton = new KeyButton
            (
                key: Keys.Escape,
                action: () =>
                {
                    WorldManager.CurWorldManager.Serialize();
                    Exit();
                }
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
                spriteBatch: new(GraphicsDevice)
            );

            CreateActiveUIManager(graphicsDevice: GraphicsDevice);
            UIRectVertPanel<Button> UIPanel = new(color: Color.White, childHorizPos: HorizPos.Middle);
            //Button newGameButton = new
            //(
            //    shape: new MyRectangle(width: 200, height: 30)
            //    {
            //        Color = Color.White
            //    },
            //    text: "New game"
            //);
            //newGameButton.clicked.Add
            //(
            //    listener: new NewGameButtonClickedListener
            //    (
            //        NewGameButton: newGameButton,
            //        GraphicsDevice: GraphicsDevice
            //    )
            //);
            //UIPanel.AddChild(child: newGameButton);

            CurActiveUIManager.AddHUDElement(HUDElement: UIPanel, horizPos: HorizPos.Middle, vertPos: VertPos.Middle);

            WorldManager.CreateWorldUIManager(graphicsDevice: GraphicsDevice);
            //WorldManager.LoadWorldUIManager(graphicsDevice: GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            exitButton.Update();

            TimeSpan elapsed = gameTime.ElapsedGameTime;

            CurActiveUIManager.Update(elapsed: elapsed);

            WorldManager.CurWorldManager?.Update(elapsed: elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            WorldManager.CurWorldManager?.Draw(graphicsDevice: GraphicsDevice);

            CurActiveUIManager.DrawHUD();

            base.Draw(gameTime);
        }
    }
}
