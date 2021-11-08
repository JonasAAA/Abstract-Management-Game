using Game1.Events;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.UI
{
    public abstract class OnOffButton : Button
    {
        [DataMember]
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

        protected OnOffButton(NearRectangle shape, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, action: null, text: text)
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
