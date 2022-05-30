using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public abstract class OnOffButton : BaseButton
    {
        public readonly Event<IOnChangedListener> onChanged;

        public bool On
        {
            get => on;
            set
            {
                if (on != value)
                {
                    on = value;
                    onChanged.Raise(action: listener => listener.OnChangedResponse());
                }
            }
        }

        protected sealed override Color Color
            => on switch
            {
                true => curUIConfig.selectedButtonColor,
                false => curUIConfig.deselectedButtonColor
            };

        private bool on;

        protected OnOffButton(NearRectangle shape, ITooltip tooltip, string text, bool on)
            : base(shape: shape, tooltip: tooltip, text: text)
        {
            onChanged = new();
            this.on = on;
        }
    }
}
