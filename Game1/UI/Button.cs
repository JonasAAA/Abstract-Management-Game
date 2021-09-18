using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class Button<TShape> : UIElement<TShape>
        where TShape : Shape
    {
        private readonly Color activeColor, passiveColor;
        private readonly TextBox textBox;
        private readonly Action action;

        public Button(TShape shape, Action action, float letterHeight, string text, Color activeColor, Color passiveColor)
            : base(shape: shape)
        {
            this.action = action;
            this.activeColor = activeColor;
            this.passiveColor = passiveColor;
            base.Shape.Color = passiveColor;
            textBox = new(letterHeight: letterHeight)
            {
                Text = text
            };
            base.Shape.CenterChanged += () => textBox.Shape.Center = base.Shape.Center;
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
            action?.Invoke();
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            Shape.Color = passiveColor;
        }
    }
}
