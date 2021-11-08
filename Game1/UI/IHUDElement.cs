namespace Game1.UI
{
    public interface IHUDElement : IUIElement
    {
        public NearRectangle Shape { get; }
    }
}
