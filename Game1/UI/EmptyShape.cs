using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class EmptyShape : Shape
    {
        public override bool Contains(Vector2 position)
            => false;

        public override void Draw()
        { }
    }
}
