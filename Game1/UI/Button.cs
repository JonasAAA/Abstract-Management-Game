using Game1.Events;
using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public class Button : HUDElement
    {
        public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;

        public Button(NearRectangle shape, string explanation = defaultExplanation, string text = null)
            : base(shape: shape, explanation)
        {
            clicked = new();
            textBox = new()
            {
                Text = text
            };
            AddChild(child: textBox);
        }

        public override IUIElement CatchUIElement(Vector2 mousePos)
        {
            if (shape.Transparent)
                throw new InvalidOperationException();
            return base.CatchUIElement(mousePos);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();
            textBox.Shape.Center = Shape.Center;
        }

        public override void OnClick()
        {
            base.OnClick();
            clicked.Raise(action: listener => listener.ClickedResponse());
        }
    }
}
