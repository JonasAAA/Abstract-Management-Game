﻿using Game1.Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class UITransparentPanel<TChild> : UIElement, IEnumerable<TChild>
        where TChild : IUIElement
    {
        public int Count
            => children.Count;

        [DataMember] private readonly List<TChild> children;

        public UITransparentPanel()
            : base(shape: new InfinitePlane())
            => children = new();

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
