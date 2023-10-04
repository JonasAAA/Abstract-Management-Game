using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class ActionButton : HUDElement, IWithTooltip
    {
        public ITooltip Tooltip { get; }

        public sealed override bool CanBeClicked
            => true;

        protected sealed override Color Color
            => colorConfig.buttonColor;

        private readonly Action action;
        private readonly TextBox textBox;

        public ActionButton(NearRectangle shape, Action action, ITooltip tooltip, string? text = null)
            : base(shape: shape)
        {
            this.action = action;
            textBox = new(text: text, textColor: colorConfig.buttonTextColor);
            Tooltip = tooltip;
            AddChild(child: textBox);
        }

        protected sealed override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();
            textBox.Shape.Center = Shape.Center;
        }

        public sealed override void OnClick()
        {
            base.OnClick();
            action();
        }
    }
}
