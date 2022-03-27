using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    public interface IHUDElement : IUIElement
    {
        public NearRectangle Shape { get; }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public void RecalcSizeAndPos();
    }
}
