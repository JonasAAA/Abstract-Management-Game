using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public interface IUIElement<out TShape> : IUIElement
        where TShape : Shape
    {
        public TShape Shape { get; }

        Shape IUIElement.GetShape()
            => Shape;
    }

    public interface IUIElement
    {
        public Field<bool> Enabled { get; }

        protected abstract Shape GetShape();

        protected virtual IEnumerable<IUIElement> GetChildren()
            => Enumerable.Empty<IUIElement>();

        public bool Contains(Vector2 position)
            => DefaultContains(UIElement: this, position: position);

        public virtual IUIElement CatchUIElement(Vector2 mousePos)
            => DefaultCatchUIElement(UIElement: this, mousePos: mousePos);

        public virtual void OnMouseEnter()
        { }

        public virtual void OnClick()
        { }

        public virtual void OnMouseLeave()
        { }

        public virtual void OnMouseDownWorldNotMe()
        { }

        public virtual void Draw()
            => DefaultDraw(UIElement: this);

        protected static bool DefaultContains(IUIElement UIElement, Vector2 position)
            => UIElement.GetShape().Contains(position: position);

        protected static IUIElement DefaultCatchUIElement(IUIElement UIElement, Vector2 mousePos)
        {
            if (!UIElement.Contains(position: mousePos))
                return null;

            foreach (var child in UIElement.GetChildren().Reverse())
            {
                var childCatchingUIElement = child.CatchUIElement(mousePos: mousePos);
                if (childCatchingUIElement is not null)
                    return childCatchingUIElement;
            }
            return UIElement.GetShape().Transparent switch
            {
                true => null,
                false => UIElement
            };
        }

        protected static void DefaultDraw(IUIElement UIElement)
        {
            UIElement.GetShape().Draw();
            foreach (var child in UIElement.GetChildren())
                child.Draw();
        }
    }
}