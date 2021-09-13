using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class UIRectElement : UIElement
    {
        public float Width
        {
            get => width;
            protected set
            {
                if (width != value)
                {
                    width = value;
                    DimensionsChanged?.Invoke();
                }
            }
        }

        public float Height
        {
            get => height;
            protected set
            {
                if (height != value)
                {
                    height = value;
                    DimensionsChanged?.Invoke();
                }
            }
        }

        public virtual Vector2 TopLeftCorner { get; set; }

        public event Action DimensionsChanged;

        private float width, height;

        public UIRectElement()
            : this(width: 0, height: 0)
        { }

        public UIRectElement(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public override bool Contains(Vector2 mousePos)
        {
            Vector2 relMousePos = mousePos - TopLeftCorner;
            return Math.Abs(relMousePos.X - width * .5f) < width * .5f && Math.Abs(relMousePos.Y - height * .5f) < height * .5f;
        }
    }
}
