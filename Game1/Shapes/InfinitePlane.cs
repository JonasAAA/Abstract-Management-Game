using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public class InfinitePlane : Shape
    {
        private static readonly Texture2D pixelTexture;

        static InfinitePlane()
            => pixelTexture = C.LoadTexture(name: "pixel");

        public InfinitePlane()
            => Color = Color.Transparent;

        public InfinitePlane(Color color)
            => Color = color;

        public override bool Contains(MyVector2 position)
            => true;

        protected override void Draw(Color color)
        {
            if (!color.Transparent())
                C.Draw
                (
                    texture: pixelTexture,
                    position: MyVector2.zero,
                    color: color,
                    rotation: 0,
                    origin: MyVector2.zero,
                    scaleX: ActiveUIManager.ScreenWidth,
                    scaleY: ActiveUIManager.ScreenHeight
                );
        }
    }
}
