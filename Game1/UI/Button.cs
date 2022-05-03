using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public class Button : HUDElement, IExplainableUIElement
    {
        // TODO: delete if not needed
        //public readonly struct ImmutableParams : IParams
        //{
        //    public string? Text { get; }
        //    public Color TextColor { get; }

        //    public ImmutableParams(string? text, Color? textColor = null)
        //    {
        //        Text = text;
        //        TextColor = textColor ?? curUIConfig.defaultTextColor;
        //    }
        //}

        public interface IParams : IExplainableParams, TextBox.IParams
        {
            Color IPanelParams.BackgroundColor
                => Color.Transparent;
        }

        public readonly Event<IClickedListener> clicked;

        public string? Explanation
            => parameters.Explanation;

        public override bool CanBeClicked
            => true;

        private readonly IParams parameters;
        protected readonly TextBox textBox;

        public Button(NearRectangle shape, IParams parameters)
            : base(shape: shape)
        {
            this.parameters = parameters;
            clicked = new();
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
            clicked.Raise(action: listener => listener.ClickedResponse());
        }
    }
}
