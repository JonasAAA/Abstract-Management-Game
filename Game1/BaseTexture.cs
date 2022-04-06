namespace Game1
{
    [Serializable]
    public class BaseTexture
    {
        public int Width
            => texture.Width;
        public int Height
            => texture.Height;

        [NonSerialized] protected Texture2D texture;
        // [x][y]
        protected readonly Color[][] colorData;
        protected readonly string textureName;

        protected BaseTexture(string textureName, Texture2D texture)
        {
            this.textureName = textureName;
            this.texture = texture;
            Color[] colorData1D = new Color[Width * Height];
            this.texture.GetData(colorData1D);
            colorData = new Color[Width][];
            for (int x = 0; x < Width; x++)
            {
                colorData[x] = new Color[Height];
                for (int y = 0; y < Height; y++)
                    colorData[x][y] = colorData1D[Ind1D(x: x, y: y, width: Width)];
            }
        }

        public bool Contains(Point pos)
            => 0 <= pos.X && pos.X < Width
            && 0 <= pos.Y && pos.Y < Height;

        public Color Color(Point pos)
            => colorData[pos.X][pos.Y];

        public void Draw(MyVector2 position, Color color, double rotation, MyVector2 origin, UDouble scale)
            => C.Draw
            (
                texture: texture,
                position: position,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale
            );

        public void Draw(MyVector2 position, Color color, double rotation, MyVector2 origin, UDouble scaleX, UDouble scaleY)
            => C.Draw
            (
                texture: texture,
                position: position,
                color: color,
                rotation: rotation,
                origin: origin,
                scaleX: scaleX,
                scaleY: scaleY
            );

        protected static int Ind1D(int x, int y, int width)
            => x + y * width;
    }
}
