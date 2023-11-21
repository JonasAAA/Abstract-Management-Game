using Game1.ContentNames;
using static Game1.GameConfig;

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

        public Icon(TextureName name)
            => image = new(name: name, height: CurGameConfig.iconHeight);

        public void Draw(Vector2Bare center)
            => image.Draw(center);
    }
}
