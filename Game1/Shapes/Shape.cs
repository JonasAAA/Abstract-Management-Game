using Game1.Events;
using Microsoft.Xna.Framework;
using System;


namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }
        public bool Transparent
           => Color.Transparent();
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

        private Vector2 center;

        protected Shape()
        {
            SizeOrPosChanged = new();
            Color = Color.Transparent;
        }

        public abstract bool Contains(Vector2 position);

        protected void RaiseSizeOrPosChanged()
            => SizeOrPosChanged.Raise(action: listener => listener.SizeOrPosChangedResponse(shape: this));

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
