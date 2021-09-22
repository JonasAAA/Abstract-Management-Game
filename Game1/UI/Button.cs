using System;
using System.Collections.Generic;

namespace Game1.UI
{
    public class Button<TShape> : UIElement<TShape>
        where TShape : Shape
    {
        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;
        private readonly Action action;

        public Button(TShape shape, Action action, float letterHeight, string text)
            : base(shape: shape)
        {
            this.action = action;
            textBox = new(letterHeight: letterHeight)
            {
                Text = text
            };
            Shape.CenterChanged += () => textBox.Shape.Center = Shape.Center;
        }

        protected override IEnumerable<IUIElement> GetChildren()
        { 
            yield return textBox;
        }

        public override void OnClick()
        {
            base.OnClick();
            action?.Invoke();
        }
    }
}
