using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.ObjectModel;

namespace Game1
{
    public static class C
    {
        public const int standardScreenHeight = 1080;
        public static double ScreenWidth
            => (double)GraphicsDevice.Viewport.Width * standardScreenHeight / GraphicsDevice.Viewport.Height;
        public static double ScreenHeight
            => standardScreenHeight;

        public const Overlay MaxRes = (Overlay)2;
        public static readonly ReadOnlyCollection<Keys> numericKeys, firstLetterKeys;
        public static ConstArray<Color> ResColors { get; private set; }
        public static ContentManager Content { get; private set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static WorldCamera WorldCamera { get; private set; }
        public static HUDCamera HUDCamera { get; private set; }

        public static readonly double minPosDouble;
        public static readonly decimal minPosDecimal;
        private static readonly Random random;

        static C()
        {
            minPosDecimal = 1e-6m;
            minPosDouble = (double)minPosDecimal;
            random = new();
            numericKeys = new
            (
                list: new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 }
            );
            firstLetterKeys = new
            (
                list: new Keys[] { Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.Y, Keys.U, Keys.I }
            );
        }

        public static void Initialize(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch, float scrollSpeed, ConstArray<Color> resColors)
        {
            C.Content = Content;
            C.GraphicsDevice = GraphicsDevice;
            SpriteBatch = spriteBatch;
            WorldCamera = new(scrollSpeed);
            HUDCamera = new();
            ResColors = resColors;
        }

        public static double Random(double min, double max)
            => min + random.NextDouble() * (max - min);

        public static Vector2 Direction(float rotation)
            => new((float)Math.Cos(rotation), (float)Math.Sin(rotation));

        public static float Rotation(Vector2 vector)
            => (float)Math.Atan2(vector.Y, vector.X);

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

        public static bool Transparent(Color color)
            => color.A is 0;

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
    }
}
