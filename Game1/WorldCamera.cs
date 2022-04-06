using Game1.Delegates;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class WorldCamera : Camera, IPosTransformer
    {
        private Matrix worldToScreen, screenToWorld;
        private UDouble scale;
        private MyVector2 worldCenter;
        private readonly MyVector2 screenCenter;

        public WorldCamera(UDouble startingWorldScale)
        {
            scale = startingWorldScale;
            worldCenter = new(0, 0);
            screenCenter = new((double)ActiveUIManager.ScreenWidth * .5, (double)ActiveUIManager.ScreenHeight * .5);
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
                double scrollDist = (double)CurWorldConfig.scrollSpeed * elapsed.TotalSeconds;
                if (ActiveUIManager.MouseHUDPos.X <= (double)CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter = worldCenter with { X = worldCenter.X - scrollDist };
                if (ActiveUIManager.MouseHUDPos.X >= (double)ActiveUIManager.ScreenWidth - (double)CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter = worldCenter with { X = worldCenter.X + scrollDist };
                if (ActiveUIManager.MouseHUDPos.Y <= (double)CurWorldConfig.screenBoundWidthForMapMoving)
                    worldCenter = worldCenter with { Y = worldCenter.Y - scrollDist };
                if (ActiveUIManager.MouseHUDPos.Y >= (double)ActiveUIManager.ScreenHeight - (double)CurWorldConfig.screenBoundWidthForMapMoving)
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
        }

        public override Matrix GetToScreenTransform()
            => worldToScreen;

        MyVector2 IPosTransformer.Transform(MyVector2 screenPos)
            => WorldPos(screenPos: screenPos);
    }
}
