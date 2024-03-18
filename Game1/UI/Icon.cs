using Game1.ContentNames;

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

        public void Draw(Vector2Bare center)
            => image.Draw(center);
    }
}
