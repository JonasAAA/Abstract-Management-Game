using Game1.Shapes;
using System.Collections;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public abstract class UIRectPanel<TChild> : HUDElement, IEnumerable<TChild>
        where TChild : IHUDElement
    {
        public int Count
            => children.Count;

        protected sealed override Color Color
            => colorConfig.UIBackgroundColor;

        protected readonly List<TChild> children;

        protected UIRectPanel()
            : base(shape: new MyRectangle())
            => children = new();

        protected void AddChildren(IEnumerable<TChild?> newChildren)
        {
            foreach (var child in newChildren)
                AddChild(child: child);
        }

        public void AddChild(TChild? child)
        {
            if (child is null)
                return;

            children.Add(child);
            base.AddChild(child: child);
        }

        public void RemoveChild(TChild? child)
        {
            if (child is null)
                return;

            children.Remove(child);
            base.RemoveChild(child: child);
        }

        public void Reinitialize(IEnumerable<TChild?> newChildren)
        {
            // Clone is needed so that don't modify the collection that am currently iterating over
            foreach (var child in children.Clone())
                RemoveChild(child: child);
            foreach (var child in newChildren)
                AddChild(child: child);
        }

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
