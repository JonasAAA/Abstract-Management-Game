using Microsoft.Xna.Framework.Content;

namespace Game1
{
    // TODO: could rename this to Helper
    public static class C
    {
        // power of two to make double / scale and double * scale lossless
        public const long accurScale = (long)1 << 10;
        public static ContentManager ContentManager
            => contentManager ?? throw new InvalidOperationException(mustInitializeMessage);
        public static GraphicsDevice GraphicsDevice
            => graphicsDevice ?? throw new InvalidOperationException(mustInitializeMessage);
        public static SpriteBatch SpriteBatch
            => spriteBatch ?? throw new InvalidOperationException(mustInitializeMessage);

        private const string mustInitializeMessage = $"must initialize {nameof(C)} first";
        private static ContentManager? contentManager;
        private static GraphicsDevice? graphicsDevice;
        private static SpriteBatch? spriteBatch;
        private static readonly Random random;

        static C()
            => random = new();

        public static void Initialize(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            C.contentManager = contentManager;
            C.graphicsDevice = graphicsDevice;
            spriteBatch = new(graphicsDevice);
        }

        /// <summary>
        /// inclusive min, exclusive max
        /// </summary>
        public static ulong Random(ulong min, ulong max)
            => (ulong)Random((int)min, (int)max);

        /// <summary>
        /// inclusive min, exclusive max
        /// </summary>
        public static int Random(int min, int max)
            => random.Next(min, max);

        public static bool RandomBool(Propor probOfTrue)
            => random.NextDouble() < (double)probOfTrue;

        public static double Random(double min, double max)
        {
            if (min > max)
                throw new ArgumentException();
            return min + random.NextDouble() * (max - min);
        }

        public static UDouble Random(UDouble min, UDouble max)
        {
            if (min > max)
                throw new ArgumentException();
            return min + (UDouble)random.NextDouble() * (UDouble)(max - min);
        }

        public static TimeSpan Random(TimeSpan min, TimeSpan max)
        {
            if (min > max)
                throw new ArgumentException();
            return min + random.NextDouble() * (max - min);
        }

        public static bool Click(bool prev, bool cur)
            => prev && !cur;

        public static Propor DonePropor(TimeSpan timeLeft, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();
            return Propor.Create(1 - timeLeft / duration) switch
            {
                Propor donePropor => donePropor,
                null => throw new ArgumentException()
            };
        }

        public static Texture2D LoadTexture(string name)
            => ContentManager.Load<Texture2D>(name);

        public static SpriteFont LoadFont(string name)
            => ContentManager.Load<SpriteFont>(name);

        public static void Draw(Texture2D texture, MyVector2 position, Color color, double rotation, MyVector2 origin, UDouble scale)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: (Vector2)position,
                sourceRectangle: null,
                color: color,
                rotation: (float)rotation,
                origin: (Vector2)origin,
                scale: (float)scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void Draw(Texture2D texture, MyVector2 position, Color color, double rotation, MyVector2 origin, UDouble scaleX, UDouble scaleY)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: (Vector2)position,
                sourceRectangle: null,
                color: color,
                rotation: (float)rotation,
                origin: (Vector2)origin,
                scale: new Vector2((float)scaleX, (float)scaleY),
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void DrawString(SpriteFont spriteFont, string text, MyVector2 position, Color color, MyVector2 origin, UDouble scale)
            => SpriteBatch.DrawString
            (
                spriteFont: spriteFont,
                text: text,
                position: (Vector2)position,
                color: color,
                rotation: 0,
                origin: (Vector2)origin,
                scale: (float)scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static bool Equals<T>(T object1, T object2)
            => EqualityComparer<T>.Default.Equals(object1, object2);

        public static Texture2D CreateTexture(int width, int height, Func<MyVector2, Color> colorFromRelToCenterPos)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();

            Texture2D texture = new(GraphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            MyVector2 textureCenter = .5 * new MyVector2(x: width, y: height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    colorData[y * width + x] = colorFromRelToCenterPos(new MyVector2(x + .5, y + .5) - textureCenter);
            texture.SetData(colorData);

            return texture;
        }
    }
}
