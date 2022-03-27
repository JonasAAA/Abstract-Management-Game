using Game1.Delegates;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class WorldCamera : Camera, IPosTransformer
    {
        private Matrix worldToScreen, screenToWorld;
        private double scale;
        private Vector2 worldCenter, screenCenter;

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
            if (canScroll)
            {
                float scrollDist = CurWorldConfig.scrollSpeed * (float)elapsed.TotalSeconds;
                if (ActiveUIManager.MouseHUDPos.X <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X -= scrollDist;
                if (ActiveUIManager.MouseHUDPos.X >= ActiveUIManager.ScreenWidth - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.X += scrollDist;
                if (ActiveUIManager.MouseHUDPos.Y <= CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y -= scrollDist;
                if (ActiveUIManager.MouseHUDPos.Y >= ActiveUIManager.ScreenHeight - CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter.Y += scrollDist;
            }

            // temporary
            double scaleChange = Math.Pow(.5, elapsed.TotalSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.O))
                scale *= scaleChange;
            if (Keyboard.GetState().IsKeyDown(Keys.I))
                scale /= scaleChange;

            worldToScreen = Matrix.CreateTranslation(-worldCenter.X * (float)ScreenScale, -worldCenter.Y * (float)ScreenScale, 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0) *
                Matrix.CreateScale((float)ScreenScale);

            screenToWorld = Matrix.Invert(worldToScreen);
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;

        Vector2 IPosTransformer.Transform(Vector2 screenPos)
            => WorldPos(screenPos: screenPos);
    }
}
