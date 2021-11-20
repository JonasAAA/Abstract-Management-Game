using Game1.Events;
using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class WorldCamera : Camera, IPosTransformer
    {
        [DataMember] private Matrix worldToScreen, screenToWorld;
        [DataMember] private readonly double scale;
        [DataMember] private Vector2 worldCenter, screenCenter;

        public WorldCamera(double startingWorldScale)
        {
            scale = startingWorldScale;
            worldCenter = new(0, 0);
            screenCenter = new((float)(ActiveUIManager.ScreenWidth * .5), (float)(ActiveUIManager.ScreenHeight * .5));
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
                if (ActiveUIManager.MouseHUDPos.X <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUIManager.MouseHUDPos.X >= ActiveUIManager.ScreenWidth - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X += CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUIManager.MouseHUDPos.Y <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y -= CurWorldConfig.scrollSpeed * elapsedSeconds;
                if (ActiveUIManager.MouseHUDPos.Y >= ActiveUIManager.ScreenHeight - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y += CurWorldConfig.scrollSpeed * elapsedSeconds;
            }

            worldToScreen = Matrix.CreateTranslation(xPosition: -worldCenter.X * (float)ScreenScale, yPosition: -worldCenter.Y * (float)ScreenScale, zPosition: 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation(xPosition: screenCenter.X, yPosition: screenCenter.Y, zPosition: 0) *
                Matrix.CreateScale((float)ScreenScale);

            screenToWorld = Matrix.Invert(worldToScreen);
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;

        Vector2 IPosTransformer.Transform(Vector2 screenPos)
            => WorldPos(screenPos: screenPos);
    }
}
