namespace Game1.UI
{
    public interface IImage
    {
        public UDouble Width { get; }
        public UDouble Height { get; }

        public void Draw(Vector2Bare center);
    }
}
