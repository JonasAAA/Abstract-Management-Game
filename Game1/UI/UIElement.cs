using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public abstract class UIElement
    {
        protected virtual IEnumerable<UIElement> Children
            => Enumerable.Empty<UIElement>();

        public abstract bool Contains(Vector2 mousePos);

        public virtual UIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(mousePos: mousePos))
                return null;

            foreach (var child in Children.Reverse())
                if (child.Contains(mousePos: mousePos))
                    return child.CatchUIElement(mousePos: mousePos);

            return this;
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
            foreach (var child in Children)
                child.Draw();
        }

        //public bool SeeThrough(Vector position);

        //public void OnHover();

        //public void OnMouseDown();

        //public void OnMouseUp(Vector relMousePos);
    }
}