namespace Game1.UI
{
    [Serializable]
    public sealed class ConfigurableIcon : IImage
    {
        public UDouble Width
            => icon.Width;
        public UDouble Height
            => icon.Height;

        private readonly Icon icon;
        private readonly IImage background;

        public ConfigurableIcon(Icon icon, IImage background)
        {
            this.icon = icon;
            if (!background.Width.IsCloseTo(icon.Width) || !background.Height.IsCloseTo(icon.Height))
                throw new ArgumentException();
            this.background = background;
        }

        public void Draw(Vector2Bare center)
        {
            background.Draw(center);
            icon.Draw(center);
        }
    }
}
