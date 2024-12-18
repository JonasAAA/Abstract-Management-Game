﻿using Game1.ContentNames;
using Microsoft.Xna.Framework.Content;

namespace Game1
{
    // TODO: could rename this to Helper
    public static class C
    {
        // power of two to make double / scale and double * scale lossless
        public const long accurScale = (long)1 << 10;
    
        public static GraphicsDevice GraphicsDevice
            => graphicsDevice ?? throw new InvalidOperationException(mustInitializeMessage);
        public static SpriteBatch SpriteBatch
            => spriteBatch ?? throw new InvalidOperationException(mustInitializeMessage);

        public static Texture2D PixelTexture
            => pixelTexture ?? throw new InvalidOperationException(mustInitializeMessage);

        public static string ContentRootDirectory
            => ContentManager.RootDirectory;

        private static ContentManager ContentManager
            => contentManager ?? throw new InvalidOperationException(mustInitializeMessage);

        private const string mustInitializeMessage = $"must initialize {nameof(C)} first";
        private static ContentManager? contentManager;
        private static GraphicsDevice? graphicsDevice;
        private static SpriteBatch? spriteBatch;
        private static Texture2D? pixelTexture;
        private static readonly Random random = new();

        public static void Initialize(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            C.contentManager = contentManager;
            C.graphicsDevice = graphicsDevice;
            spriteBatch = new(graphicsDevice);
            pixelTexture = LoadTexture(name: TextureName.pixel);
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

        public static Propor DonePropor(TimeSpan timeLeft, TimeSpan duration)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);
            return Propor.Create(1 - timeLeft / duration) switch
            {
                Propor donePropor => donePropor,
                null => throw new ArgumentException()
            };
        }

        public static Texture2D LoadTexture(TextureName name)
            => ContentManager.Load<Texture2D>(name.Path);

        public static SpriteFont LoadFont(FontName name)
            => ContentManager.Load<SpriteFont>(name.Path);

        public static Effect LoadShader(ShaderName name)
            => ContentManager.Load<Effect>(name.Path);

        public static void Draw(Texture2D texture, Vector2Bare position, Color color, double rotation, Vector2Bare origin, UDouble scale)
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

        public static void Draw(Texture2D texture, Vector2Bare position, Color color, double rotation, Vector2Bare origin, UDouble scaleX, UDouble scaleY)
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

        public static void Draw(Texture2D texture, MyVector2 position, Color color, double rotation, Vector2Bare origin, Length scale)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: (Vector2)position,
                sourceRectangle: null,
                color: color,
                rotation: (float)rotation,
                origin: (Vector2)origin,
                scale: (float)scale.valueInM,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void Draw(Texture2D texture, MyVector2 position, Color color, double rotation, Vector2Bare origin, Length scaleX, Length scaleY)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: (Vector2)position,
                sourceRectangle: null,
                color: color,
                rotation: (float)rotation,
                origin: (Vector2)origin,
                scale: new Vector2((float)scaleX.valueInM, (float)scaleY.valueInM),
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void DrawString(SpriteFont spriteFont, string text, Vector2Bare position, Color color, Vector2Bare origin, UDouble scale)
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

        public static Texture2D CreateTexture(int width, int height, Func<Vector2Bare, Color> colorFromRelToCenterPos)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();

            Texture2D texture = new(GraphicsDevice, width, height);
            var colorData = new Color[width * height];
            Vector2Bare textureCenter = .5 * new Vector2Bare(x: width, y: height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    colorData[y * width + x] = colorFromRelToCenterPos(new Vector2Bare(x + .5, y + .5) - textureCenter);
            texture.SetData(colorData);

            return texture;
        }

        public static Color ColorFromRGB(int rgb)
            => new(r: (rgb >> 16) & 255, g: (rgb >> 8) & 255, b: rgb & 255);
    }
}
