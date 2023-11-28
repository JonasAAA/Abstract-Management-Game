using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class Button<TVisual> : BaseButton<TVisual>
        where TVisual : IHUDElement
    {
        protected sealed override Color Color { get; }

        public Button(NearRectangle shape, TVisual visual, ITooltip tooltip, Color? color = null)
            : base(shape: shape, visual: visual, tooltip: tooltip)
            => Color = color ?? colorConfig.buttonColor;
    }
}
