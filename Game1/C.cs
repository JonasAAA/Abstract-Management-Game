using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public static class C
    {
        public static int ScreenWidth
            => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static int ScreenHeight
            => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        public static ContentManager Content { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static GameTime GameTime { get; private set; }
        public static Camera Camera { get; private set; }

        private static readonly double minPosDouble;
        private static readonly Random random;

        static C()
        {
            //ScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //ScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            minPosDouble = 1e-6;
            random = new();
        }

        public static void Initialize(float scrollSpeed, ContentManager Content, SpriteBatch spriteBatch)
        {
            Camera = new(scrollSpeed);
            C.Content = Content;
            SpriteBatch = spriteBatch;
        }

        public static void Update(GameTime gameTime)
            => C.GameTime = gameTime;

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
    }
}
