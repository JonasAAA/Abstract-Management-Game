using Game1.UI;
using System.Diagnostics.CodeAnalysis;

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

        public static void Initialize()
            => screenScale = (double)C.GraphicsDevice.Viewport.Height / ActiveUIManager.curUIConfig.standardScreenHeight;

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

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Want to keep consistent signature with BeginDraw method")]
        public void EndDraw()
            => C.SpriteBatch.End();
    }
}
