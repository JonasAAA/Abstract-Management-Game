using System.Diagnostics.CodeAnalysis;

namespace Game1.UI
{
    [Serializable]
    public sealed class HUDCamera : Camera
    {
        private readonly Matrix HUDToScreen, screenToHUD;

        public HUDCamera()
        {
            HUDToScreen = Matrix.CreateScale(scale: (float)ScreenScale);
            screenToHUD = Matrix.Invert(HUDToScreen);
        }

        public MyVector2 ScreenPosToHUDPos(MyVector2 screenPos)
            => MyVector2.Transform(position: screenPos, matrix: screenToHUD);

        public MyVector2 HUDPosToScreenPos(MyVector2 HUDPos)
            => MyVector2.Transform(position: HUDPos, matrix: HUDToScreen);

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is so that the player doesn't need to care about internal implementation details")]
        public UDouble HUDLengthToScreenLength(UDouble HUDLength)
            => HUDLength * ScreenScale;

        public sealed override Matrix GetToScreenTransform()
            => HUDToScreen;
    }
}
