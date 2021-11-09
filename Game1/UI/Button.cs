using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class Button : HUDElement
    {
        [DataMember] public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        [DataMember] protected readonly TextBox textBox;

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
