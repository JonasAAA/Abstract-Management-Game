using Game1.Shapes;
using System.Collections;

namespace Game1.UI
{
    [Serializable]
    public abstract class UIRectPanel<TChild> : HUDElement, IEnumerable<TChild>
        where TChild : IHUDElement
    {
        public int Count
            => children.Count;

        protected readonly List<TChild> children;

        protected UIRectPanel(Color color)
            : base(shape: new MyRectangle(color: color))
        {
            children = new();
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

        public IEnumerator<TChild> GetEnumerator()
            => children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
