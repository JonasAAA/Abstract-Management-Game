using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

namespace Game1
{
    [DataContract]
    public class WorldCamera : Camera
    {
        [DataMember] private Matrix worldToScreen, screenToWorld;
        [DataMember] private readonly double scale;
        [DataMember] private Vector2 worldCenter, screenCenter;

        public WorldCamera(GraphicsDevice graphicsDevice)
            : base(graphicsDevice: graphicsDevice)
        {
            scale = CurWorldConfig.startingWorldScale;
            worldCenter = new(0, 0);
            screenCenter = new((float)(CurActiveUIManager.screenWidth * .5), (float)(CurActiveUIManager.ScreenHeight * .5));
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
                if (CurActiveUIManager.MouseHUDPos.X <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (CurActiveUIManager.MouseHUDPos.X >= CurActiveUIManager.screenWidth - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X += CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (CurActiveUIManager.MouseHUDPos.Y <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (CurActiveUIManager.MouseHUDPos.Y >= CurActiveUIManager.ScreenHeight - CurWorldConfig.screenBoundWidthForMapMoving)
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
