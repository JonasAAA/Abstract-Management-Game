using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class HUDElement : UIElement<IHUDElement>, IHUDElement, ISizeOrPosChangedListener
    {
        public abstract class Params : IHUDElement.IParams, ISizeChangedListener
        {
            public Event<ISizeChangedListener> SizeChanged
                => ShapeParams.SizeChanged;

            public NearRectangle.Params ShapeParams { get; }

            protected abstract Color Color { get; }

            private readonly List<IHUDElement.IParams> children;
            private bool inRecalcSize;

            protected Params(NearRectangle.Params shapeParams)
            {
                ShapeParams = shapeParams;
                SizeChanged.Add(listener: this);
                children = new();
                inRecalcSize = false;
            }

            protected void AddChild(IHUDElement.IParams child)
            {
                children.Add(child);
                child.SizeChanged.Add(listener: this);
                RecalcSize();
            }

            protected void RemoveChild(IHUDElement.IParams child)
            {
                if (!children.Remove(child))
                    throw new ArgumentException();
                child.SizeChanged.Remove(listener: this);
                RecalcSize();
            }

            public void RecalcSize()
            {
                if (inRecalcSize)
                    return;
                inRecalcSize = true;

                PartOfRecalcSize();
                foreach (var child in children)
                    child.RecalcSize();

                inRecalcSize = false;
            }

            protected virtual void PartOfRecalcSize()
            {
                if (!inRecalcSize)
                    throw new InvalidOperationException();
            }

            void ISizeChangedListener.SizeChangedResponse(Shape.Params shapeParams)
                => RecalcSize();
        }

        public NearRectangle Shape { get; }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged
            => Shape.SizeOrPosChanged;

        private readonly Params parameters;
        private readonly List<IHUDElement> children;
        private bool inRecalcSizeAndPos;

        protected HUDElement(Params parameters)
            : base(shape: parameters.ShapeParams.CreateShape())
        {
            Shape = shape;
            this.parameters = parameters;
            this.children = ;
            SizeOrPosChanged.Add(listener: this);
            parameters.SizeChanged.Add(listener: this);
            inRecalcSizeAndPos = false;
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
