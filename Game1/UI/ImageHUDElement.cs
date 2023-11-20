using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class ImageHUDElement : HUDElement, IMaybeWithTooltip
    {
        public ITooltip? Tooltip { get; }

        protected sealed override Color Color { get; }

        private readonly IImage image;

        public ImageHUDElement(IImage image)
            : base(shape: new MyRectangle(width: image.Width, height: image.Height))
        {
            this.image = image;
            Tooltip = image is IMaybeWithTooltip withTooltip ? withTooltip.Tooltip : null;
            // This color will generally not be visible
            // Just want it not to be transparent so that it captures clicks
            Color = colorConfig.UIBackgroundColor;
        }

        protected sealed override void DrawChildren()
        {
            base.DrawChildren();
            image.Draw(center: Shape.Center);
        }
    }
}
