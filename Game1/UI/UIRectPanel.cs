using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIRectPanel : UIRectElement
    {
        protected override IEnumerable<UIElement> Children
            => children;

        protected readonly List<UIRectElement> children;

        protected UIRectPanel()
            => children = new();

        public void AddChild(UIRectElement child)
        {
            SetNewChildCoords(child: child);
            children.Add(child);
            child.DimensionsChanged += RecalcDimensions;
            RecalcDimensions();
        }

        protected abstract void SetNewChildCoords(UIRectElement child);

        protected abstract void RecalcDimensions();
    }
}
