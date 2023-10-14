using Game1.Shapes;
using System.Collections;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public abstract class UIRectPanel<TChildParams> : HUDElement
        where TChildParams : IHUDElement.IParams
    {
        public abstract new class Params : HUDElement.Params, IEnumerable<TChildParams>
        {
            public int Count
                => children.Count;

            protected sealed override Color Color
                => colorConfig.UIBackgroundColor;

            // new list here as want children to be TChildParams rather than IHUDElement.IParams
            protected readonly List<TChildParams> children;

            protected Params()
                : base(shapeParams: new(width: 0, height: 0))
                => children = new();

            protected void AddChildren(IEnumerable<TChildParams?> newChildren)
            {
                foreach (var child in newChildren)
                    AddChild(child: child);
            }

            public void AddChild(TChildParams? child)
            {
                if (child is null)
                    return;

                children.Add(child);
                base.AddChild(child: child);
            }

            public void RemoveChild(TChildParams? child)
            {
                if (child is null)
                    return;

                children.Remove(child);
                base.RemoveChild(child: child);
            }

            public void Reinitialize(IEnumerable<TChildParams?> newChildren)
            {
                // Clone is needed so that don't modify the collection that am currently iterating over
                foreach (var child in children.Clone())
                    RemoveChild(child: child);
                foreach (var child in newChildren)
                    AddChild(child: child);
            }

            public IEnumerator<TChildParams> GetEnumerator()
                => children.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        protected UIRectPanel(Params parameters)
            : base(parameters: parameters, shape: new MyRectangle())
        { }
    }
}
