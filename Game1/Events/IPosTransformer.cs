using Microsoft.Xna.Framework;

namespace Game1.Events
{
    public interface IPosTransformer
    {
        public Vector2 Transform(Vector2 screenPos);
    }
}
