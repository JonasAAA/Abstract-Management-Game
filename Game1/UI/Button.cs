using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class Button<TShape> : UIElement<TShape>
        where TShape : Shape
    {
        private readonly Action action;
        private readonly Color activeColor, passiveColor;
        private readonly TextBox textBox;

        public Button(TShape shape, float letterHeight, string text, Action action, Color activeColor, Color passiveColor)
            : base(shape: shape)
        {
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

        protected override IEnumerable<UIElement> GetChildren()
        { 
            yield return textBox;
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            Shape.Color = activeColor;
        }

        public override void OnClick()
        {
            base.OnClick();
            action();
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            Shape.Color = passiveColor;
        }
    }
}
