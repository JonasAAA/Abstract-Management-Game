using Game1.Shapes;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public abstract class UIRectPanel<TChild> : HUDElement, IEnumerable<TChild>
        where TChild : IHUDElement
    {
        public int Count
            => children.Count;

        [DataMember] protected readonly List<TChild> children;

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
