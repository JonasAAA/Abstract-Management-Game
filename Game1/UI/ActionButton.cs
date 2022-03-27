using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ActionButton : HUDElement
    {
        public override bool CanBeClicked
            => true;

        private readonly Action action;
        private readonly TextBox textBox;

        public ActionButton(NearRectangle shape, Action action, string text = null, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
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
            action();
        }
    }
}
