using Game1.Shapes;

namespace Game1.Delegates
{
    public interface ISizeOrPosChangedListener : IListener
    {
        public void SizeOrPosChangedResponse(Shape shape);
    }
}
