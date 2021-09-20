using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class Button<TShape> : IUIElement<TShape>
        where TShape : Shape
    {
        public TShape Shape { get; }
        public Field<bool> Enabled { get; }

        private readonly Color activeColor, passiveColor;
        private readonly TextBox textBox;
        private readonly Action action;

        public Button(TShape shape, Action action, float letterHeight, string text, Color activeColor, Color passiveColor)
        {
            Shape = shape;
            Enabled = new(value: true);
            this.action = action;
            this.activeColor = activeColor;
            this.passiveColor = passiveColor;
            Shape.Color = passiveColor;
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

        public void OnMouseEnter()
        {
            //base.OnMouseEnter();
            Shape.Color = activeColor;
        }

        public void OnClick()
        {
            //base.OnClick();
            action?.Invoke();
        }

        public void OnMouseLeave()
        {
            //base.OnMouseLeave();
            Shape.Color = passiveColor;
        }
    }
}
