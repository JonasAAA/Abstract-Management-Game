using Microsoft.Xna.Framework;

namespace Game1
{
    public interface IUIElement
    {
        public bool Contains(Vector2 position);

        public void ActiveUpdate();
    }
}
