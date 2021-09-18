using Microsoft.Xna.Framework;

namespace Game1
{
    public sealed class WorldCamera : Camera
    {
        private Matrix worldToScreen, screenToWorld;
        private readonly float scrollSpeed, boundWidth;
        private readonly double scale;
        private Vector2 worldCenter, screenCenter;

        public WorldCamera(float scrollSpeed)
        {
            this.scrollSpeed = scrollSpeed;
            scale = 1;
            boundWidth = 10;
            worldCenter = new(0, 0);
            screenCenter = new((float)(C.ScreenWidth * .5), (float)(C.ScreenHeight * .5));
            Update(canScroll: false);
        }

        public Vector2 WorldPos(Vector2 screenPos)
            => Vector2.Transform(position: screenPos, matrix: screenToWorld);

        public Vector2 ScreenPos(Vector2 worldPos)
            => Vector2.Transform(position: worldPos, matrix: worldToScreen);

        public void Update(bool canScroll)
        {
            if (canScroll)
            {
                if (MyMouse.HUDPos.X <= boundWidth)
                    worldCenter.X -= scrollSpeed;
                if (MyMouse.HUDPos.X >= C.ScreenWidth - boundWidth)
                    worldCenter.X += scrollSpeed;
                if (MyMouse.HUDPos.Y <= boundWidth)
                    worldCenter.Y -= scrollSpeed;
                if (MyMouse.HUDPos.Y >= C.ScreenHeight - boundWidth)
                    worldCenter.Y += scrollSpeed;
            }

            worldToScreen = Matrix.CreateTranslation(xPosition: -worldCenter.X * (float)screenScale, yPosition: -worldCenter.Y * (float)screenScale, zPosition: 0) *
                Matrix.CreateScale((float)scale) *
                Matrix.CreateTranslation(xPosition: screenCenter.X, yPosition: screenCenter.Y, zPosition: 0) *
                Matrix.CreateScale((float)screenScale);

            screenToWorld = Matrix.Invert(worldToScreen);
        }

        protected override Matrix GetToScreenTransform()
            => worldToScreen;
    }
}
