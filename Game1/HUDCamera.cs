using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class HUDCamera : Camera
    {
        private readonly Matrix HUDToScreen, screenToHUD;

        public HUDCamera(GraphicsDevice graphicsDevice)
            : base(graphicsDevice: graphicsDevice)
        {
            HUDToScreen = Matrix.CreateScale(scale: (float)screenScale);
            screenToHUD = Matrix.Invert(HUDToScreen);
        }

        public Vector2 HUDPos(Vector2 screenPos)
            => Vector2.Transform(position: screenPos, matrix: screenToHUD);

        public override Matrix GetToScreenTransform()
            => HUDToScreen;
    }
}
