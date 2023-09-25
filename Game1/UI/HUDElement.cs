using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class HUDElement : UIElement<IHUDElement>, IHUDElement, ISizeOrPosChangedListener
    {
        public NearRectangle Shape { get; }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged
            => Shape.SizeOrPosChanged;

        private bool inRecalcSizeAndPos;

        protected HUDElement(NearRectangle shape)
            : base(shape: shape)
        {
            Shape = shape;
            SizeOrPosChanged.Add(listener: this);
            inRecalcSizeAndPos = false;
        }

        protected sealed override void AddChild(IHUDElement child, ulong layer = 0)
        {
            base.AddChild(child, layer);
            child.SizeOrPosChanged.Add(listener: this);
            RecalcSizeAndPos();
        }

        protected sealed override void RemoveChild(IHUDElement child)
        {
            base.RemoveChild(child);
            child.SizeOrPosChanged.Remove(listener: this);
            RecalcSizeAndPos();
        }

        public void RecalcSizeAndPos()
        {
            if (inRecalcSizeAndPos)
                return;
            inRecalcSizeAndPos = true;

            PartOfRecalcSizeAndPos();
            foreach (var child in Children())
                child.RecalcSizeAndPos();

            inRecalcSizeAndPos = false;
        }

        protected virtual void PartOfRecalcSizeAndPos()
        {
            if (!inRecalcSizeAndPos)
                throw new InvalidOperationException();
        }

        public virtual void SizeOrPosChangedResponse(Shape shape)
            => RecalcSizeAndPos();
    }
}
