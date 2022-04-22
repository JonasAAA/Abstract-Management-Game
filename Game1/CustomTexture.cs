using System.Runtime.Serialization;

namespace Game1
{
    [Serializable]
    public class CustomTexture : BaseTexture
    {
        private static Texture2D CloneTexture(string textureName)
        {
            Texture2D copyFromTexture = C.LoadTexture(textureName);
            Color[] colorData1D = new Color[copyFromTexture.Width * copyFromTexture.Height];
            copyFromTexture.GetData(colorData1D);

            Texture2D copyToTexture = new(C.GraphicsDevice, width: copyFromTexture.Width, copyFromTexture.Height);
            copyToTexture.SetData(colorData1D);

            return copyToTexture;
        }

        private int minEditX, maxEditX, minEditY, maxEditY;
        private bool editing;

        public CustomTexture(string textureName)
            : base(textureName: textureName, texture: CloneTexture(textureName: textureName))
        {
            editing = false;
        }

        public void StartEdit()
        {
            if (editing)
                throw new InvalidOperationException();
            editing = true;

            minEditX = Width;
            maxEditX = 0;
            minEditY = Height;
            maxEditY = 0;

        }

        public void DrawLineInTexture(Point pos1, Point pos2, Color color)
        {
            if (!editing)
                throw new InvalidOperationException();

            if (!Contains(pos: pos1) || !Contains(pos: pos2))
                throw new ArgumentException();
            
            if (pos1 == pos2)
                SetColor(pos: pos1, color: color);

            int xDiff = pos2.X - pos1.X,
                yDiff = pos2.Y - pos1.Y;
            if (MyMathHelper.Abs(xDiff) >= MyMathHelper.Abs(yDiff))
            {
                // x increment
                int xIncr = MyMathHelper.Sign(xDiff);
                for (int x = pos1.X; ; x += xIncr)
                {
                    int y = Convert.ToInt32(pos1.Y + (double)(pos2.Y - pos1.Y) * (x - pos1.X) / xDiff);
                    SetColor(pos: new Point(x, y), color: color);
                    if (x == pos2.X)
                        break;
                }
            }
            else
            {
                // y increment
                int yIncr = MyMathHelper.Sign(yDiff);
                for (int y = pos1.Y; ; y += yIncr)
                {
                    int x = Convert.ToInt32(pos1.X + (double)(pos2.X - pos1.X) * (y - pos1.Y) / yDiff);
                    SetColor(pos: new Point(x, y), color: color);
                    if (y == pos2.Y)
                        break;
                }
            }
        }
        
        private void SetColor(Point pos, Color color)
        {
            if (!editing)
                throw new InvalidOperationException();
            minEditX = MyMathHelper.Min(minEditX, pos.X);
            maxEditX = MyMathHelper.Max(maxEditX, pos.X);
            minEditY = MyMathHelper.Min(minEditY, pos.Y);
            maxEditY = MyMathHelper.Max(maxEditY, pos.Y);
            colorData[pos.X][pos.Y] = color;
        }

        public void EndEdit()
        {
            if (!editing)
                throw new InvalidOperationException();
            editing = false;

            if (minEditX > maxEditX || minEditY > maxEditY)
                return;

            Rectangle rect = new(minEditX, minEditY, maxEditX - minEditX + 1, maxEditY - minEditY + 1);
            Color[] rectColorData1D = new Color[rect.Width * rect.Height];
            for (int x = minEditX; x <= maxEditX; x++)
                for (int y = minEditY; y <= maxEditY; y++)
                    rectColorData1D[Ind1D(x: x - minEditX, y: y - minEditY, width: rect.Width)] = colorData[x][y];
            texture.SetData
            (
                level: 0,
                rect: rect,
                data: rectColorData1D,
                startIndex: 0,
                elementCount: rect.Width * rect.Height
            );
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            int width = colorData.Length,
                height = colorData[0].Length;
            texture = new(C.GraphicsDevice, width, height);
            Color[] colorData1D = new Color[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    colorData1D[Ind1D(x: x, y: y, width: Width)] = colorData[x][y];
            texture.SetData(colorData1D);
        }
    }
}
