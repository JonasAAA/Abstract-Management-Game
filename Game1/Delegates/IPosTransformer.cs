using Microsoft.Xna.Framework;

namespace Game1.Delegates
{
    public interface IPosTransformer
    {
        public Vector2 Transform(Vector2 screenPos);
    }
}
