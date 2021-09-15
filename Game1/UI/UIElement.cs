using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public abstract class UIElement<TShape> : UIElement
        where TShape : Shape
    {
        public readonly TShape Shape;

        public UIElement(TShape shape)
            => this.Shape = shape;

        protected override Shape GetShape()
            => Shape;
    }

    public abstract class UIElement
    {
        protected abstract Shape GetShape();

        protected virtual IEnumerable<UIElement> GetChildren()
            => Enumerable.Empty<UIElement>();

        public bool Contains(Vector2 position)
            => GetShape().Contains(position: position);

        public virtual UIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(position: mousePos))
                return null;

            foreach (var child in GetChildren().Reverse())
            {
                var childCatchingUIElement = child.CatchUIElement(mousePos: mousePos);
                if (childCatchingUIElement is not null)
                    return childCatchingUIElement;
            }
            return GetShape().Transparent switch
            {
                true => null,
                false => this
            };
        }

        public virtual void OnMouseEnter()
        { }

        public virtual void OnClick()
        { }

        public virtual void OnMouseLeave()
        { }

        public virtual void OnMouseDownWorldNotMe()
        { }

        public virtual void Draw()
        {
            GetShape().Draw();
            foreach (var child in GetChildren())
                child.Draw();
        }

        //public bool SeeThrough(Vector position);

        //public void OnHover();

        //public void OnMouseDown();

        //public void OnMouseUp(Vector relMousePos);
    }
}