using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Game1.UI
{
    public abstract class UIRectPanel<TChild> : HUDElement/*<MyRectangle>*/, IEnumerable<TChild>
        where TChild : IHUDElement/*<NearRectangle>*/
    {
        public int Count
            => children.Count;

        protected readonly List<TChild> children;

        protected UIRectPanel(Color color)
            : base(shape: new MyRectangle())
        {
            Shape.Color = color;
            children = new();
        }

        public void AddChild(TChild child)
        {
            if (child is null)
                return;

            children.Add(child);
            base.AddChild(child: child);
        }

        public void RemoveChild(TChild child)
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
