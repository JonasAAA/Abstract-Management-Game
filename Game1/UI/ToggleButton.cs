using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class ToggleButton<TShape> : IUIElement<TShape>
        where TShape : NearRectangle
    {
        public TShape Shape { get; }

        public Field<bool> Enabled { get; }

        public bool On
        {
            get => on;
            set
            {
                if (on != value)
                {
                    on = value;
                    Shape.Color = GetColor();
                    OnChanged?.Invoke();
                }
            }
        }

        public event Action OnChanged;

        private bool on;
        private readonly Color mouseOnColor, selectedColor, deselectedColor;
        private readonly TextBox textBox;

        public ToggleButton(TShape shape, float letterHeight, bool on, string text, Color mouseOnColor, Color selectedColor, Color deselectedColor)
        {
            Shape = shape;
            Enabled = new(value: true);
            this.on = on;
            this.mouseOnColor = mouseOnColor;
            this.selectedColor = selectedColor;
            this.deselectedColor = deselectedColor;
            Shape.Color = GetColor();

            textBox = new(letterHeight: letterHeight)
            {
                Text = text
            };
            Shape.CenterChanged += () => textBox.Shape.Center = Shape.Center;
        }

        IEnumerable<IUIElement> IUIElement.GetChildren()
        {
            yield return textBox;
        }

        public void OnClick()
        {
            //base.OnClick();
            On = !On;
            Shape.Color = Color.Lerp(mouseOnColor, GetColor(), .5f);
        }

        public void OnMouseEnter()
        {
            //base.OnMouseEnter();

            Shape.Color = Color.Lerp(mouseOnColor, GetColor(), .5f);
        }

        public void OnMouseLeave()
        {
            //base.OnMouseLeave();

            Shape.Color = GetColor();
        }

        private Color GetColor()
            => on switch
            {
                true => selectedColor,
                false => deselectedColor
            };
    }
}
