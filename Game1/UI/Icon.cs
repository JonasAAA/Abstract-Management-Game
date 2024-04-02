using Game1.ContentNames;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class Icon : IImage
    {
        public UDouble Width
            => image.Width;
        public UDouble Height
            => image.Height;

        private readonly Image image;

        public Icon(TextureName name, UDouble height)
            => image = new(name: name, height: height);

        public ConfigurableIcon WithDefaultBackgroundColor()
            => new
            (
                icon: this,
                background: new ColorRect
                (
                    width: Width,
                    height: Height,
                    color: colorConfig.defaultIconBackgroundColor
                )
            );

        public ConfigurableIcon WithMatPaletteNotYetChosenBackgroundColor()
            => new
            (
                icon: this,
                background: new ColorRect
                (
                    width: Width,
                    height: Height,
                    color: colorConfig.matPaletteNotYetChosenBackgroundColor
                )
            );


        public void Draw(Vector2Bare center)
            => image.Draw(center);
    }
}
