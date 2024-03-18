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

        /// <param name="backgroundColor">
        ///     This color will generally not be visible, except where image had transparent background.
        ///     The important part is to make it non-transparent (as is default) if want image to capture clicks.
        /// </param>
        public ImageHUDElement(IImage image, Color? backgroundColor = null)
            : base(shape: new MyRectangle(width: image.Width, height: image.Height))
        {
            this.image = image;
            Tooltip = image is IMaybeWithTooltip withTooltip ? withTooltip.Tooltip : null;
            Color = backgroundColor ?? colorConfig.iconBackgroundColor;
        }

        protected sealed override void DrawChildren()
        {
            base.DrawChildren();
            image.Draw(center: Shape.Center);
        }
    }
}
