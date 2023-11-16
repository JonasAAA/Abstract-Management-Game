using Game1.Shapes;
using System.Collections;

namespace Game1.UI
{
    [Serializable]
    public sealed class UITransparentPanel<TChild> : UIElement<IUIElement>, IEnumerable<TChild>
        where TChild : IUIElement
    {
        public int Count
            => children.Count;

        protected sealed override Color Color
            => Color.Transparent;

        private readonly List<TChild> children;

        public UITransparentPanel()
            : base(shape: new InfinitePlane())
            => children = [];

        public void AddChild(TChild child, ulong layer = 0)
        {
            children.Add(child);
            base.AddChild(child: child, layer: layer);
        }

        public void RemoveChild(TChild child)
        {
            children.Remove(child);
            base.RemoveChild(child: child);
        }

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
