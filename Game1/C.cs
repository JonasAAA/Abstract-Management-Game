using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game1
{
    public static class C
    {
        // power of two to make float / scale and float * scale lossless
        public const long accurScale = (long)1 << 10;
        public static ContentManager contentManager { get; private set; }
        public static GraphicsDevice graphicsDevice { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        //private static ContentManager contentManager;
        private static readonly double minPosDouble;
        private static readonly decimal minPosDecimal;
        private static readonly Random random;

        static C()
        {
            minPosDecimal = 1e-6m;
            //minPosDouble = (double)minPosDecimal;
            minPosDouble = .0001;
            random = new();
        }

        public static void Initialize(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            C.contentManager = contentManager;
            C.graphicsDevice = graphicsDevice;
            SpriteBatch = new(graphicsDevice);
        }

        public static double Random(double min, double max)
        {
            if (min > max)
                throw new ArgumentException();
            return min + random.NextDouble() * (max - min);
        }

        public static TimeSpan Random(TimeSpan min, TimeSpan max)
        {
            if (min > max)
                throw new ArgumentException();
            return min + random.NextDouble() * (max - min);
        }

        public static Vector2 Direction(float rotation)
            => new((float)Math.Cos(rotation), (float)Math.Sin(rotation));

        public static float Rotation(Vector2 vector)
            => (float)Math.Atan2(vector.Y, vector.X);

        /// <summary>
        /// 90 degrees to the left
        /// </summary>
        public static Vector2 OrthDir(Vector2 direction)
            => new(direction.Y, -direction.X);

        public static bool Click(bool prev, bool cur)
            => prev && !cur;

        public static bool IsTiny(double value)
            => Math.Abs(value) < minPosDouble;

        public static bool IsTiny(decimal value)
            => Math.Abs(value) < minPosDecimal;

        public static double DonePart(TimeSpan timeLeft, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();
            if (timeLeft > duration)
                throw new ArgumentException();
            return Math.Min(1, 1 - timeLeft / duration);
        }

        public static bool IsInSuitableRange(double value)
            => value is >= 0 and <= 1;

        public static bool IsSuitable(double value)
            => value is double.NegativeInfinity || IsInSuitableRange(value: value);

        //public static bool Transparent(Color color)
        //    => color.A is 0;

        public static Texture2D LoadTexture(string name)
            => contentManager.Load<Texture2D>(name);

        public static SpriteFont LoadFont(string name)
            => contentManager.Load<SpriteFont>(name);

        public static void Draw(Texture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, float scale)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: position,
                sourceRectangle: null,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void Draw(Texture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
            => SpriteBatch.Draw
            (
                texture: texture,
                position: position,
                sourceRectangle: null,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, Vector2 origin, float scale)
            => SpriteBatch.DrawString
            (
                spriteFont: spriteFont,
                text: text,
                position: position,
                color: color,
                rotation: 0,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );

        public static bool Equals<T>(T object1, T object2)
            => EqualityComparer<T>.Default.Equals(object1, object2);
    }
}
