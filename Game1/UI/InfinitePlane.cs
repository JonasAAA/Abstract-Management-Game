using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class InfinitePlane : Shape
    {
        public InfinitePlane()
            => Color = Color.Transparent;

        public override bool Contains(Vector2 position)
            => true;

        protected override void Draw(Color color)
        { }
    }
}
