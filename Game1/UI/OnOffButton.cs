using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public abstract class OnOffButton<TVisual> : BaseButton<TVisual>
        where TVisual : IHUDElement
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
                true => colorConfig.selectedButtonColor,
                false => colorConfig.deselectedButtonColor
            };

        private bool on;

        protected OnOffButton(NearRectangle shape, TVisual visual, ITooltip tooltip, bool on)
            : base(shape: shape, visual: visual, tooltip: tooltip)
        {
            onChanged = new();
            this.on = on;
        }
    }
}
