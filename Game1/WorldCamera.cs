using Game1.UI;

namespace Game1
{
    [Serializable]
    public sealed class WorldCamera : Camera
    {
        public static UDouble GetWorldScaleFromCameraViewHeight(UDouble cameraViewHeight)
            => ActiveUIManager.curUIConfig.standardScreenHeight / cameraViewHeight;
        public static UDouble CameraViewWidthFromHeight(UDouble cameraViewHeight)
            => ActiveUIManager.screenWidth / ActiveUIManager.screenHeight * cameraViewHeight;

        public MyVector2 WorldCenter { get; private set; }
        
        public UDouble CameraViewHeight
            => ActiveUIManager.curUIConfig.standardScreenHeight / scale;

        private Matrix worldToScreen, screenToWorld;
        private UDouble scale;
        private readonly MyVector2 screenCenter;
        private readonly UDouble scrollSpeed, screenBoundWidthForMapMoving;

        public WorldCamera(MyVector2 worldCenter, UDouble startingWorldScale, UDouble scrollSpeed, UDouble screenBoundWidthForMapMoving)
        {
            WorldCenter = worldCenter;
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
            => worldLength * scale * screenScale;

        public UDouble ScreenLengthToWorldLength(UDouble screenLength)
            => screenLength / (scale * screenScale);

        public void MoveTo(MyVector2 worldCenter, UDouble worldScale)
        {
            WorldCenter = worldCenter;
            scale = worldScale;
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public void Update(TimeSpan elapsed, bool canScroll)
        {
            if (canScroll)
            {
                double scrollDist = scrollSpeed * elapsed.TotalSeconds / scale;
                if (isCoordNonnegAndSmall(value: ActiveUIManager.MouseHUDPos.X))
                    WorldCenter = WorldCenter with { X = WorldCenter.X - scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.screenWidth - ActiveUIManager.MouseHUDPos.X))
                    WorldCenter = WorldCenter with { X = WorldCenter.X + scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.MouseHUDPos.Y))
                    WorldCenter = WorldCenter with { Y = WorldCenter.Y - scrollDist };
                if (isCoordNonnegAndSmall(value: ActiveUIManager.screenHeight - ActiveUIManager.MouseHUDPos.Y))
                    WorldCenter = WorldCenter with { Y = WorldCenter.Y + scrollDist };
            }

            // temporary
            UDouble scaleChange = MyMathHelper.Pow((UDouble).5, elapsed.TotalSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.O))
                scale *= scaleChange;
            if (Keyboard.GetState().IsKeyDown(Keys.I))
                scale /= scaleChange;

            worldToScreen = Matrix.CreateTranslation(-(float)WorldCenter.X, -(float)WorldCenter.Y, 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation((float)screenCenter.X, (float)screenCenter.Y, 0) *
                Matrix.CreateScale((float)screenScale);

            screenToWorld = Matrix.Invert(worldToScreen);

            return;

            // care about nonnegativity so that the scrolling works appropriately in multi-screen setup
            bool isCoordNonnegAndSmall(double value)
                => 0 <= value && value <= screenBoundWidthForMapMoving; 
        }

        public sealed override Matrix GetToScreenTransform()
            => worldToScreen;
    }
}
