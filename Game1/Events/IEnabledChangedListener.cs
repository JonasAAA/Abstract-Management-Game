using Game1.UI;

namespace Game1.Events
{
    public interface IEnabledChangedListener : IListener
    {
        public void EnabledChangedResponse(IUIElement UIElement);
    }
}
