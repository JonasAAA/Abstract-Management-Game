using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class Button : BaseButton
    {
        protected override Color Color { get; }

        public Button(NearRectangle shape, ITooltip tooltip, string? text = null, Color? color = null)
            : base(shape: shape, tooltip: tooltip, text: text)
            => Color = color ?? colorConfig.buttonColor;

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();
            textBox.Shape.Center = Shape.Center;
        }
    }
}
