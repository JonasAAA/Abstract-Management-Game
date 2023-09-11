using Game1.Shapes;

namespace Game1.Delegates
{
    public interface ISizeOrPosChangedListener
    {
        public void SizeOrPosChangedResponse(Shape shape);
    }
}
