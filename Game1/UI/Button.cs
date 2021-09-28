using System;

namespace Game1.UI
{
    public class Button<TShape> : UIElement<TShape>
        where TShape : Shape
    {
        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;
        private readonly Action action;

        public Button(TShape shape, string explanation = defaultExplanation, Action action = null, string text = null)
            : base(shape: shape, explanation)
        {
            this.action = action;
            textBox = new()
            {
                Text = text
            };
            AddChild(child: textBox);
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
