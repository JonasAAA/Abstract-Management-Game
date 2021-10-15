namespace Game1.UI
{
    public class HUDElement<TShape> : UIElement, IHUDElement<TShape>
        where TShape : NearRectangle
    {
        public TShape Shape { get; }

        public HUDElement(TShape shape, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
        {
            Shape = shape;
            Initialize();
        }
    }
}
