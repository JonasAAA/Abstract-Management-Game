namespace Game1.UI
{
    public interface IHUDElement/*<out TShape>*/ : IUIElement
        //where TShape : NearRectangle
    {
        public NearRectangle Shape { get; }
        //public TShape Shape { get; }
    }
}
