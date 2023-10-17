using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class InfinitePlane : Shape
    {
        public sealed override bool Contains(Vector2Bare screenPos)
            => true;

        public sealed override void Draw(Color color)
        {
            if (!color.Transparent())
                C.Draw
                (
                    texture: C.PixelTexture,
                    position: Vector2Bare.zero,
                    color: color,
                    rotation: 0,
                    origin: Vector2Bare.zero,
                    scaleX: ActiveUIManager.screenWidth,
                    scaleY: ActiveUIManager.screenHeight
                );
        }
    }
}
