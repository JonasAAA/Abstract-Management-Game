using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class OnOffButton<TShape> : Button<TShape>
        where TShape : NearRectangle
    {
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
                    OnChanged?.Invoke();
                }
            }
        }

        public event Action OnChanged;

        private bool on;
        private readonly Color selectedColor, deselectedColor;

        protected OnOffButton(TShape shape, float letterHeight, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, action: null, letterHeight: letterHeight, text: text)
        {
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
