using Game1.Shapes;

namespace Game1.Delegates
{
    public interface ISizeChangedListener
    {
        public void SizeChangedResponse(Shape.Params shapeParams);
    }
}
