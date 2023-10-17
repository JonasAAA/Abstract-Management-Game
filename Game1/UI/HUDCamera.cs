using System.Diagnostics.CodeAnalysis;

namespace Game1.UI
{
    [Serializable]
    public sealed class HUDCamera : Camera
    {
        private readonly Matrix HUDToScreen, screenToHUD;

        public HUDCamera()
        {
            HUDToScreen = Matrix.CreateScale(scale: (float)screenScale);
            screenToHUD = Matrix.Invert(HUDToScreen);
        }

        public Vector2Bare ScreenPosToHUDPos(Vector2Bare screenPos)
            => (Vector2Bare)Vector2.Transform(position: (Vector2)screenPos, matrix: screenToHUD);

        public Vector2Bare HUDPosToScreenPos(Vector2Bare HUDPos)
            => (Vector2Bare)Vector2.Transform(position: (Vector2)HUDPos, matrix: HUDToScreen);

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is so that the player doesn't need to care about internal implementation details")]
        public UDouble HUDLengthToScreenLength(UDouble HUDLength)
            => HUDLength * screenScale;

        public sealed override Matrix GetToScreenTransform()
            => HUDToScreen;
    }
}
