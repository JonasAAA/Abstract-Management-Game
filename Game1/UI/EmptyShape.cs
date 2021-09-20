using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class EmptyShape : NearRectangle
    {
        public EmptyShape()
            : base(width: 0, height: 0)
        { }

        public override bool Contains(Vector2 position)
            => false;

        public override void Draw()
        { }
    }
}
