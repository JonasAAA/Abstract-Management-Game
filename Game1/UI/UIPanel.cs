using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIPanel : UIElement
    {
        protected readonly List<UIElement> children;

        protected UIPanel()
            => children = new();

        public void AddChild(UIElement child)
        {
            children.Add(child);
            child.DimensionsChanged += RecalcDimensions;
            RecalcDimensions();
        }

        protected abstract void RecalcDimensions();

        public override UIElement CatchUIElement(Vector2 mousePos)
        {
            if (!Contains(mousePos: mousePos))
                return null;

            foreach (var child in children)
                if (child.Contains(mousePos: mousePos))
                    return child.CatchUIElement(mousePos: mousePos);

            return this;
        }

        public override void Draw()
        {
            children.ForEach(child => child.Draw());
        }
    }
}
