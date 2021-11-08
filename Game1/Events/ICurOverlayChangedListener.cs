namespace Game1.Events
{
    public interface ICurOverlayChangedListener : IListener
    {
        public void OverlayChangedResponse(Overlay oldOverlay);
    }
}
