using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class UIElement
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

        public UIElement()
            : this(width: 0, height: 0)
        { }

        public UIElement(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public virtual bool Contains(Vector2 mousePos)
        {
            Vector2 relMousePos = mousePos - TopLeftCorner;
            return Math.Abs(relMousePos.X - width * .5f) < width * .5f && Math.Abs(relMousePos.Y - height * .5f) < height * .5f;
        }

        public virtual UIElement CatchUIElement(Vector2 mousePos)
            => Contains(mousePos: mousePos) switch
            {
                true => this,
                false => null
            };

        public virtual void OnClick()
        { }

        public abstract void Draw();

        //public abstract bool SeeThrough(Vector position);

        //public abstract void OnHover();

        //public abstract void OnMouseDown();

        //public abstract void OnMouseUp(Vector relMousePos);
    }
}
