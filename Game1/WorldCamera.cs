using Game1.UI;

namespace Game1
{
    [Serializable]
    public sealed class WorldCamera : Camera
    {
        public static Length GetWorldMetersPerPixelFromCameraViewHeight(Length cameraViewHeight)
            => cameraViewHeight / ActiveUIManager.curUIConfig.standardScreenHeight;
        public static Length CameraViewWidthFromHeight(Length cameraViewHeight)
            => ActiveUIManager.screenWidth / ActiveUIManager.screenHeight * cameraViewHeight;

        public MyVector2 WorldCenter { get; private set; }
        
        public Length CameraViewHeight
            => ActiveUIManager.curUIConfig.standardScreenHeight * worldMetersPerPixel;

        private Matrix worldToScreen, screenToWorld;
        private Length worldMetersPerPixel;
        private readonly Vector2Bare screenCenter;
        private readonly UDouble scrollSpeed;
        private readonly UDouble screenBoundWidthForMapMoving;

        public WorldCamera(MyVector2 worldCenter, Length worldMetersPerPixel, UDouble scrollSpeed, UDouble screenBoundWidthForMapMoving)
        {
            WorldCenter = worldCenter;
            this.worldMetersPerPixel = worldMetersPerPixel;
            this.scrollSpeed = scrollSpeed;
            this.screenBoundWidthForMapMoving = screenBoundWidthForMapMoving;
            screenCenter = new(ActiveUIManager.screenWidth * .5, ActiveUIManager.screenHeight * .5);
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public MyVector2 ScreenPosToWorldPos(Vector2Bare screenPos)
            => (MyVector2)Vector2.Transform(position: (Vector2)screenPos, matrix: screenToWorld);

        public Vector2Bare WorldPosToScreenPos(MyVector2 worldPos)
            => (Vector2Bare)Vector2.Transform(position: (Vector2)worldPos, matrix: worldToScreen);

        public UDouble WorldLengthToScreenLength(Length worldLength)
            => worldLength * screenScale / worldMetersPerPixel;

        public Length ScreenLengthToWorldLength(UDouble screenLength)
            => screenLength * worldMetersPerPixel / screenScale;

        public void MoveTo(MyVector2 worldCenter, Length worldMetersPerPixel)
        {
            WorldCenter = worldCenter;
            this.worldMetersPerPixel = worldMetersPerPixel;
            Update(elapsed: TimeSpan.Zero, canScroll: false);
        }

        public void Update(TimeSpan elapsed, bool canScroll)
        {
            if (canScroll)
            {
                Length scrollDist = scrollSpeed * (UDouble)elapsed.TotalSeconds * worldMetersPerPixel;
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
            UDouble scaleChange = MyMathHelper.Pow(UDouble.half, elapsed.TotalSeconds);
            if (Keyboard.GetState().IsKeyDown(Keys.O))
                worldMetersPerPixel /= scaleChange;
            if (Keyboard.GetState().IsKeyDown(Keys.I))
                worldMetersPerPixel *= scaleChange;
            worldToScreen = Matrix.CreateTranslation(-(float)WorldCenter.X.valueInM, -(float)WorldCenter.Y.valueInM, 0) *
                Matrix.CreateScale((float)(1 / worldMetersPerPixel.valueInM)) *
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
