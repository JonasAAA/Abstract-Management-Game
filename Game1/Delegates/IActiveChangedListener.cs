using Game1.UI;

namespace Game1.Delegates
{
    public interface IActiveChangedListener : IListener
    {
        public void ActiveChangedResponse(WorldUIElement worldUIElement);
    }
}
