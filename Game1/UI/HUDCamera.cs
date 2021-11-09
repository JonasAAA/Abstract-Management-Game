using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class HUDCamera : Camera
    {
        [DataMember] private readonly Matrix HUDToScreen, screenToHUD;

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
