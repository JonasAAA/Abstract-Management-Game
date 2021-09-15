using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIRectPanel : UIElement<MyRectangle>
    {
        protected readonly List<UIElement<MyRectangle>> children;

        protected UIRectPanel(Color color)
            : base(shape: new())
        {
            Shape.Color = color;
            children = new();
        }

        protected override IEnumerable<UIElement> GetChildren()
            => children;

        public void AddChild(UIElement<MyRectangle> child)
        {
            SetNewChildCoords(child: child);
            children.Add(child);
            child.Shape.WidthChanged += RecalcWidth;
            child.Shape.HeightChanged += RecalcHeight;
            RecalcWidth();
            RecalcHeight();
        }

        protected abstract void SetNewChildCoords(UIElement<MyRectangle> child);

        protected abstract void RecalcWidth();

        protected abstract void RecalcHeight();
    }
}
