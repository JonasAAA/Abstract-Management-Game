using Game1.Shapes;

namespace Game1.UI
{
    // TODO: may rename this and other names which mention ActionButton as it no longer requires action
    [Serializable]
    public class ActionButton : HUDElement, IExplainableUIElement
    {
        public interface IParams : IExplainableParams, TextBox.IParams
        {
            public void OnClick();

            Color IPanelParams.BackgroundColor
                => Color.Transparent;
        }

        public string? Explanation
            => parameters.Explanation;

        public override bool CanBeClicked
            => true;

        private readonly IParams parameters;
        private readonly TextBox textBox;

        public ActionButton(NearRectangle shape, IParams parameters)
            : base(shape: shape)
        {
            this.parameters = parameters;
            textBox = new(parameters: parameters);
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
            parameters.OnClick();
        }
    }
}
