﻿using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class Button : HUDElement/*, IWithTooltip*/
    {
        public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;

        public Button(NearRectangle shape, string? text = null)
            : base(shape: shape)
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
