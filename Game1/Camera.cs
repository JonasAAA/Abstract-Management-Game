using Game1.UI;

namespace Game1
{
    [Serializable]
    public abstract class Camera
    {
        protected static double ScreenScale
        {
            get
            {
                if (screenScale is 0)
                    throw new InvalidOperationException();
                return screenScale;
            }
        }

        private static double screenScale;

        public static void Initialize(GraphicsDevice graphicsDevice)
            => screenScale = (double)graphicsDevice.Viewport.Height / ActiveUIManager.curUIConfig.standardScreenHeight;

        public void BeginDraw()
            => C.SpriteBatch.Begin
            (
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: null,
                depthStencilState: null,
                rasterizerState: null,
                effect: null,
                transformMatrix: GetToScreenTransform()
            );

        public abstract Matrix GetToScreenTransform();

        public void EndDraw()
            => C.SpriteBatch.End();
    }
}
