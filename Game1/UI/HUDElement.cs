namespace Game1.UI
{
    public class HUDElement : UIElement, IHUDElement
    {
        public NearRectangle Shape { get; }

        public HUDElement(NearRectangle shape, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
        {
            Shape = shape;
            Initialize();
        }
    }
}
