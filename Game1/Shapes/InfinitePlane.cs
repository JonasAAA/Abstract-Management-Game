using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class InfinitePlane : Shape
    {
        public override bool Contains(MyVector2 position)
            => true;

        public override void Draw(Color color)
        {
            if (!color.Transparent())
                C.Draw
                (
                    texture: C.PixelTexture,
                    position: MyVector2.zero,
                    color: color,
                    rotation: 0,
                    origin: MyVector2.zero,
                    scaleX: ActiveUIManager.screenWidth,
                    scaleY: ActiveUIManager.screenHeight
                );
        }
    }
}
