using Microsoft.Xna.Framework;
using System;
using System.Linq;

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
                if (!C.IsTiny(value: Vector2.Distance(center, value)))
                {
                    center = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }

        public event Action SizeOrPosChanged
        {
            add
            {
                if (sizeOrPosChanged is not null && sizeOrPosChanged.GetInvocationList().Contains(value))
                    throw new InvalidOperationException();
                sizeOrPosChanged -= value;
                sizeOrPosChanged += value;
            }
            remove
            {
                sizeOrPosChanged -= value;
            }
        }

        private Vector2 center;

        private event Action sizeOrPosChanged;

        protected Shape()
            => Color = Color.Transparent;

        public abstract bool Contains(Vector2 position);

        protected void RaiseSizeOrPosChanged()
            => sizeOrPosChanged?.Invoke();

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!C.Transparent(color: Color))
                Draw(color: Color);
        }

        public void Draw(Color otherColor, float otherColorProp)
        {
            Color color = Color.Lerp(Color, otherColor, amount: otherColorProp);
            if (!C.Transparent(color: color))
                Draw(color: color);
        }
    }
}
