using Game1.Delegates;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class WorldCamera : Camera
    {
        private Matrix worldToScreen, screenToWorld;
        private UDouble scale;
        private MyVector2 worldCenter;
        private readonly MyVector2 screenCenter;

        public WorldCamera(UDouble startingWorldScale)
        {
            scale = startingWorldScale;
            worldCenter = new(0, 0);
            screenCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public MyVector2 WorldPos(MyVector2 screenPos)
            => MyVector2.Transform(position: screenPos, matrix: screenToWorld);

        public MyVector2 ScreenPos(MyVector2 worldPos)
            => MyVector2.Transform(position: worldPos, matrix: worldToScreen);

        public void Update(TimeSpan elapsed, bool canScroll)
        {
            if (canScroll)
            {
                double scrollDist = CurWorldConfig.scrollSpeed * elapsed.TotalSeconds;
                if (isCoordNonnegAndSmall(value: ActiveUIManager.MouseHUDPos.X))
                    worldCenter = worldCenter with { X = worldCenter.X - scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.screenWidth - ActiveUIManager.MouseHUDPos.X))
                    worldCenter = worldCenter with { X = worldCenter.X + scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.MouseHUDPos.Y))
                    worldCenter = worldCenter with { Y = worldCenter.Y - scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.screenHeight - ActiveUIManager.MouseHUDPos.Y))
                    worldCenter = worldCenter with { Y = worldCenter.Y + scrollDist };
            }

            // temporary
            UDouble scaleChange = MyMathHelper.Pow((UDouble).5, elapsed.TotalSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.O))
                scale *= scaleChange;
            if (Keyboard.GetState().IsKeyDown(Keys.I))
                scale /= scaleChange;

            worldToScreen = Matrix.CreateTranslation((float)(-worldCenter.X * ScreenScale), (float)(-worldCenter.Y * ScreenScale), 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation((float)screenCenter.X, (float)screenCenter.Y, 0) *
                Matrix.CreateScale((float)ScreenScale);

            screenToWorld = Matrix.Invert(worldToScreen);

            return;

            // care about nonnegativity so that the scrolling works appropriately in multi-screen setup
            static bool isCoordNonnegAndSmall(double value)
                => 0 <= value && value <= CurWorldConfig.screenBoundWidthForMapMoving; 
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;
    }
}
