using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public abstract class Camera
    {
        [DataMember] protected readonly double screenScale;

        protected Camera(GraphicsDevice graphicsDevice)
            => screenScale = (double)graphicsDevice.Viewport.Height / ActiveUIManager.CurUIConfig.standardScreenHeight;

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
