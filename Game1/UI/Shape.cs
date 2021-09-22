using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class Shape
    {
        public bool Transparent
           => C.Transparent(color: Color);
        public Color Color { get; set; }
        public Vector2 Center
        {
            get => center;
            set
            {
                if (center != value)
                {
                    center = value;
                    CenterChanged?.Invoke();
                }
            }
        }

        public event Action CenterChanged;

        private Vector2 center;

        protected Shape()
            => Color = Color.Transparent;

        public abstract bool Contains(Vector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
            => Draw(color: Color);

        public void Draw(Color otherColor, float otherColorProp)
            => Draw
            (
                color: Color.Lerp(Color, otherColor, amount: otherColorProp)
            );
    }
}
