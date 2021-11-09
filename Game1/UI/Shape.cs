using Game1.Events;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public abstract class Shape
    {
        [DataMember] public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; private init; }
        public bool Transparent
           => C.Transparent(color: Color);
        [DataMember] public Color Color { get; set; }
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

        [DataMember] private Vector2 center;

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
            if (!C.Transparent(color: Color))
                Draw(color: Color);
        }

        public void Draw(Color otherColor, float otherColorProp)
        {
            Color color = Color.Lerp(Color, otherColor, amount: otherColorProp);
            color.A = Color.A;
            if (!C.Transparent(color: color))
                Draw(color: color);
        }
    }
}
