using System.Collections;
using System.Collections.Generic;

namespace Game1.UI
{
    public class UITransparentPanel<TChild> : UIElement, IEnumerable<TChild>
        where TChild : IUIElement
    {
        public int Count
            => children.Count;

        private readonly List<TChild> children;

        public UITransparentPanel()
            : base(shape: new InfinitePlane())
            => children = new();

        public void AddChild(TChild child, int layer = 0)
        {
            children.Add(child);
            base.AddChild(child: child, layer: layer);
        }

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
