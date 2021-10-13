using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public abstract class Camera
    {
        protected readonly double screenScale;

        protected Camera()
            => screenScale = (double)C.GraphicsDevice.Viewport.Height / ActiveUI.UIConfig.standardScreenHeight;

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
