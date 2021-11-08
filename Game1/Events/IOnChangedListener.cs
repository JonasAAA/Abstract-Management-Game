using Game1.UI;

namespace Game1.Events
{
    public interface IOnChangedListener : IListener
    {
        public void OnChangedResponse();
    }
}
