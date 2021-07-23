using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public sealed class Camera
    {
        private Matrix worldToScreen, screenToWorld;
        private readonly float scrollSpeed;
        private readonly float scale;
        private Vector2 worldCenter, screenCenter;

        public Camera(float scrollSpeed)
        {
            this.scrollSpeed = scrollSpeed;
            scale = 1;
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
            if (MyMouse.ScreenPos.X <= 5)
                worldCenter.X -= scrollSpeed;
            if (MyMouse.ScreenPos.X >= C.ScreenWidth - 5)
                worldCenter.X += scrollSpeed;
            if (MyMouse.ScreenPos.Y <= 5)
                worldCenter.Y -= scrollSpeed;
            if (MyMouse.ScreenPos.Y >= C.ScreenHeight - 5)
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
