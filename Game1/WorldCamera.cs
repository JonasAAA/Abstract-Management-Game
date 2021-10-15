using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Game1.WorldManager;

namespace Game1
{
    public sealed class WorldCamera : Camera
    {
        private Matrix worldToScreen, screenToWorld;
        private readonly double scale;
        private Vector2 worldCenter, screenCenter;

        public WorldCamera(GraphicsDevice graphicsDevice)
            : base(graphicsDevice: graphicsDevice)
        {
            scale = CurWorldConfig.startingWorldScale;
            worldCenter = new(0, 0);
            screenCenter = new((float)(ActiveUI.ScreenWidth * .5), (float)(ActiveUI.ScreenHeight * .5));
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public Vector2 WorldPos(Vector2 screenPos)
            => Vector2.Transform(position: screenPos, matrix: screenToWorld);

        public Vector2 ScreenPos(Vector2 worldPos)
            => Vector2.Transform(position: worldPos, matrix: worldToScreen);

        public void Update(TimeSpan elapsed, bool canScroll)
        {
            float elapsedSeconds = (float)elapsed.TotalSeconds;
            if (canScroll)
            {
                if (ActiveUI.HUDPos.X <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUI.HUDPos.X >= ActiveUI.ScreenWidth - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X += CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUI.HUDPos.Y <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUI.HUDPos.Y >= ActiveUI.ScreenHeight - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y += CurWorldConfig.scrollSpeed * elapsedSeconds;
            }

            worldToScreen = Matrix.CreateTranslation(xPosition: -worldCenter.X * (float)screenScale, yPosition: -worldCenter.Y * (float)screenScale, zPosition: 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation(xPosition: screenCenter.X, yPosition: screenCenter.Y, zPosition: 0) *
                Matrix.CreateScale((float)screenScale);

            screenToWorld = Matrix.Invert(worldToScreen);
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;
    }
}
