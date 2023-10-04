using Game1.Delegates;
using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public abstract class BaseButton : HUDElement, IWithTooltip
    {
        public string? Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        public ITooltip Tooltip { get; }

        public readonly Event<IClickedListener> clicked;

        public override bool CanBeClicked
            => true;

        protected readonly TextBox textBox;

        protected BaseButton(NearRectangle shape, ITooltip tooltip, string? text = null)
            : base(shape: shape)
        {
            clicked = new();
            textBox = new(text: text, textColor: colorConfig.buttonTextColor);
            Tooltip = tooltip;
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
