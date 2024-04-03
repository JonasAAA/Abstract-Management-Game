using Game1.Shapes;
using static Game1.GlobalTypes.GameConfig;

namespace Game1.UI
{
    [Serializable]
    public sealed class ColorRect : IImage
    {
        public static ColorRect CreateIconSized(Color color)
            => new(width: CurGameConfig.iconWidth, height: CurGameConfig.iconHeight, color: color);

        public static ColorRect CreateSmallIconSized(Color color)
            => new(width: CurGameConfig.smallIconWidth, height: CurGameConfig.smallIconHeight, color: color);

        public UDouble Width { get; }
        public UDouble Height { get; }

        private readonly Color color;

        public ColorRect(UDouble width, UDouble height, Color color)
        {
            Width = width;
            Height = height;
            this.color = color;
        }

        public void Draw(Vector2Bare center)
            => new MyRectangle(width: Width, height: Height)
            {
                Center = center
            }.Draw(color: color);
    }
}
