using Game1.UI;

namespace Game1.Events
{
    public interface ISizeOrPosChangedListener : IListener
    {
        public void SizeOrPosChangedResponse(Shape shape);
    }
}
