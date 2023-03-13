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

        public UDouble HUDLengthToScreenLength(UDouble HUDLength)
            => HUDLength * ScreenScale;

        public override Matrix GetToScreenTransform()
            => HUDToScreen;
    }
}
