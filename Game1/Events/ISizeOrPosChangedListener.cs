using Game1.Shapes;

namespace Game1.Events
{
    public interface ISizeOrPosChangedListener : IListener
    {
        public void SizeOrPosChangedResponse(Shape shape);
    }
}
