using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIRectPanel<TChild> : IUIElement<MyRectangle>, IEnumerable<TChild>
        where TChild : IUIElement<NearRectangle>
    {
        public MyRectangle Shape { get; }
        public Field<bool> Enabled { get; }
        public int Count
            => children.Count;

        protected readonly List<TChild> children;

        protected UIRectPanel(Color color)
        {
            Shape = new()
            {
                Color = color
            };
            Enabled = new(value: true);
            Shape.CenterChanged += RecalcChildrenPos;
            children = new();
        }

        IEnumerable<IUIElement> IUIElement.GetChildren()
            => (IEnumerable<IUIElement>)children;

        public void AddChild(TChild child)
        {
            SetNewChildCoords(child: child);
            children.Add(child);
            child.Shape.WidthChanged += RecalcWidth;
            child.Shape.WidthChanged += RecalcChildrenPos;
            child.Shape.HeightChanged += RecalcHeight;
            child.Shape.HeightChanged += RecalcChildrenPos;
            RecalcWidth();
            RecalcHeight();
        }

        protected abstract void SetNewChildCoords(TChild child);

        protected abstract void RecalcChildrenPos();

        protected abstract void RecalcWidth();

        protected abstract void RecalcHeight();

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
