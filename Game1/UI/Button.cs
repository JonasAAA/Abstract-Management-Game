using Game1.Events;
using Game1.Shapes;
using System;


namespace Game1.UI
{
    [Serializable]
    public class Button : HUDElement
    {
        public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;

        public Button(NearRectangle shape, string text = null, string explanation = defaultExplanation)
            : base(shape: shape, explanation)
        {
            clicked = new();
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
            clicked.Raise(action: listener => listener.ClickedResponse());
        }
    }
}
