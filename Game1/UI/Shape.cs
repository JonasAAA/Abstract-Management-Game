using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class Shape
    {
        public bool Transparent
            => Color.A is 0;
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

        public abstract void Draw();
    }
}
