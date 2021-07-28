using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public sealed class Camera
    {
        private Matrix worldToScreen, screenToWorld;
        private readonly float scrollSpeed, scale, boundWidth;
        private Vector2 worldCenter, screenCenter;

        public Camera(float scrollSpeed)
        {
            this.scrollSpeed = scrollSpeed;
            scale = 1;
            boundWidth = 30;
            worldCenter = new(0, 0);
            screenCenter = new(C.ScreenWidth * .5f, C.ScreenHeight * .5f);
            Update();
        }

        public Vector2 Position(Vector2 screenPos)
            => Vector2.Transform(position: screenPos, matrix: screenToWorld);

        public Vector2 ScreenPos(Vector2 position)
            => Vector2.Transform(position: position, matrix: worldToScreen);

        public void Update()
        {
            if (MyMouse.ScreenPos.X <= boundWidth)
                worldCenter.X -= scrollSpeed;
            if (MyMouse.ScreenPos.X >= C.ScreenWidth - boundWidth)
                worldCenter.X += scrollSpeed;
            if (MyMouse.ScreenPos.Y <= boundWidth)
                worldCenter.Y -= scrollSpeed;
            if (MyMouse.ScreenPos.Y >= C.ScreenHeight - boundWidth)
                worldCenter.Y += scrollSpeed;

            worldToScreen = Matrix.CreateTranslation(xPosition: -worldCenter.X, yPosition: -worldCenter.Y, zPosition: 0) *
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(xPosition: screenCenter.X, yPosition: screenCenter.Y, zPosition: 0);
            screenToWorld = Matrix.Invert(worldToScreen);
        }

        public void BeginDraw()
            => C.SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: null,
                depthStencilState: null, rasterizerState: null, effect: null, transformMatrix: worldToScreen);

        public void EndDraw()
            => C.SpriteBatch.End();
    }
}
