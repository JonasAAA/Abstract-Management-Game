using Game1.Events;
using Microsoft.Xna.Framework;
using System;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public bool Transparent
           => Color.Transparent();
        public Color Color { get; set; }
        public virtual Vector2 Center { get; set; }

        protected Shape()
            => Color = Color.Transparent;

        public abstract bool Contains(Vector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!Color.Transparent())
                Draw(color: Color);
        }

        public void Draw(Color otherColor, float otherColorProp)
        {
            Color color = Color.Lerp(Color, otherColor, amount: otherColorProp);
            color.A = Color.A;
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
