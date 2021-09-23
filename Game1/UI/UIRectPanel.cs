using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIRectPanel<TChild> : UIElement<MyRectangle>, IEnumerable<TChild>
        where TChild : IUIElement<NearRectangle>
    {
        public int Count
            => children.Count;

        protected readonly List<TChild> children;

        protected UIRectPanel(Color color)
            : base(shape: new())
        {
            Shape.Color = color;
            children = new();
        }

        public void AddChild(TChild child)
        {
            children.Add(child);
            base.AddChild(child: child);
        }

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
