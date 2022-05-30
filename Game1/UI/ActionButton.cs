using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class ActionButton : HUDElement, IWithTooltip
    {
        public ITooltip Tooltip { get; }

        public override bool CanBeClicked
            => true;

        protected override Color Color
            => curUIConfig.buttonColor;

        private readonly Action action;
        private readonly TextBox textBox;

        public ActionButton(NearRectangle shape, Action action, ITooltip tooltip, string? text = null)
            : base(shape: shape)
        {
            this.action = action;
            textBox = new()
            {
                Text = text
            };
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
            action();
        }
    }
}
