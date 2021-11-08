using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public class Button : HUDElement
    {
        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;
        private readonly Action action;

        public Button(NearRectangle shape, string explanation = defaultExplanation, Action action = null, string text = null)
            : base(shape: shape, explanation)
        {
            this.action = action;
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
            action?.Invoke();
        }
    }
}
