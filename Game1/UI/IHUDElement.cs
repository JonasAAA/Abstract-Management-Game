namespace Game1.UI
{
    public interface IHUDElement<out TShape> : IUIElement
        where TShape : NearRectangle
    {
        public TShape Shape { get; }
    }
}
