using Game1.UI;

namespace Game1
{
    [Serializable]
    public sealed class WorldCamera : Camera
    {
        public static UDouble GetWorldScaleFromCameraViewHeight(UDouble cameraViewHeight)
            => ActiveUIManager.curUIConfig.standardScreenHeight / cameraViewHeight;

        private Matrix worldToScreen, screenToWorld;
        private UDouble scale;
        private MyVector2 worldCenter;
        private readonly MyVector2 screenCenter;
        private readonly UDouble scrollSpeed, screenBoundWidthForMapMoving;

        public WorldCamera(MyVector2 worldCenter, UDouble startingWorldScale, UDouble scrollSpeed, UDouble screenBoundWidthForMapMoving)
        {
            this.worldCenter = worldCenter;
            scale = startingWorldScale;
            this.scrollSpeed = scrollSpeed;
            this.screenBoundWidthForMapMoving = screenBoundWidthForMapMoving;
            screenCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public MyVector2 ScreenPosToWorldPos(MyVector2 screenPos)
            => MyVector2.Transform(position: screenPos, matrix: screenToWorld);

        public MyVector2 WorldPosToScreenPos(MyVector2 worldPos)
            => MyVector2.Transform(position: worldPos, matrix: worldToScreen);

        public UDouble WorldLengthToScreenLength(UDouble worldLength)
            => worldLength * scale * ScreenScale;

        public UDouble ScreenLengthToWorldLength(UDouble screenLength)
            => screenLength / (scale * ScreenScale);

        public void Update(TimeSpan elapsed, bool canScroll)
        {
            if (canScroll)
            {
                double scrollDist = scrollSpeed * elapsed.TotalSeconds / scale;
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
            bool isCoordNonnegAndSmall(double value)
                => 0 <= value && value <= screenBoundWidthForMapMoving; 
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;
    }
}
