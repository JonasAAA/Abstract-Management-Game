using Game1.UI;

namespace Game1.Events
{
    public interface IActiveChangedListener : IListener
    {
        public void ActiveChangedResponse(WorldUIElement worldUIElement);
    }
}
