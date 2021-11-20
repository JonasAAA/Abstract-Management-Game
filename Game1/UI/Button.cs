using Game1.Events;
using Game1.Shapes;
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
