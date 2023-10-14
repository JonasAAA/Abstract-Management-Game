using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    public interface IHUDElement : IUIElement
    {
        public interface IParams
        {
            public Event<ISizeChangedListener> SizeChanged { get; }

            public NearRectangle.Params ShapeParams { get; }

            public void RecalcSize();
        }

        public NearRectangle Shape { get; }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public void RecalcSizeAndPos();
    }
}
