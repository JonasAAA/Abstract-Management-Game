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

        public MyVector2 HUDPos(MyVector2 screenPos)
            => MyVector2.Transform(position: screenPos, matrix: screenToHUD);

        public override Matrix GetToScreenTransform()
            => HUDToScreen;
    }
}
