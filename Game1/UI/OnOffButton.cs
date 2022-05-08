using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class OnOffButton : Button
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
                    Shape.Color = on switch
                    {
                        true => selectedColor,
                        false => deselectedColor
                    };
                    onChanged.Raise(action: listener => listener.OnChangedResponse());
                }
            }
        }

        private bool on;
        private readonly Color selectedColor, deselectedColor;

        protected OnOffButton(NearRectangle shape, ITooltip tooltip, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, tooltip: tooltip, text: text)
        {
            onChanged = new();
            this.on = on;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;
            Shape.Color = on switch
            {
                true => selectedColor,
                false => deselectedColor
            };
        }
    }
}
