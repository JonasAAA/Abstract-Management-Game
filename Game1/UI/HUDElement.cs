using Game1.Events;
using Game1.Shapes;
using System;

namespace Game1.UI
{
    [Serializable]
    public class HUDElement : UIElement<IHUDElement>, IHUDElement, ISizeOrPosChangedListener
    {
        public NearRectangle Shape { get; }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged
            => Shape.SizeOrPosChanged;

        private bool inRecalcSizeAndPos;

        public HUDElement(NearRectangle shape, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
        {
            Shape = shape;
            SizeOrPosChanged.Add(listener: this);
            inRecalcSizeAndPos = false;
        }

        protected override void AddChild(IHUDElement child, ulong layer = 0)
        {
            base.AddChild(child, layer);
            child.SizeOrPosChanged.Add(listener: this);
            RecalcSizeAndPos();
        }

        protected override void RemoveChild(IHUDElement child)
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
