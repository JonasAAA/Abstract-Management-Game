using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public static class C
    {
        public static uint ScreenWidth
            => (uint)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static uint ScreenHeight
            => (uint)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        public static ConstArray<Color> ResColors { get; private set; }
        public static ContentManager Content { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static TimeSpan TotalGameTime { get; private set; }
        public static TimeSpan ElapsedGameTime { get; private set; }
        public static TimeSpan TotalGameTimeForPowerPlants { get; private set; }
        public static TimeSpan ElapsedGameTimeForPowerPlants { get; private set; }
        public static Camera Camera { get; private set; }

        public static readonly double minPosDouble;
        private static readonly Random random;

        static C()
        {
            minPosDouble = 1e-6;
            random = new();
        }

        public static void Initialize(float scrollSpeed, ContentManager Content, SpriteBatch spriteBatch, ConstArray<Color> resColors)
        {
            Camera = new(scrollSpeed);
            C.Content = Content;
            SpriteBatch = spriteBatch;
            ResColors = resColors;
        }

        public static void Update(TimeSpan elapsed, double energyProp)
        {
            if (!IsInSuitableRange(value: energyProp))
                throw new ArgumentOutOfRangeException();

            ElapsedGameTime = elapsed * energyProp;
            TotalGameTime += ElapsedGameTime;

            ElapsedGameTimeForPowerPlants = elapsed;
            TotalGameTimeForPowerPlants += ElapsedGameTimeForPowerPlants;
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

        public static double DonePart(TimeSpan endTime, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();
            if (TotalGameTime + duration < endTime)
                throw new ArgumentException();
            return Math.Min(1 - (endTime - TotalGameTime) / duration, 1);
        }

        public static bool IsInSuitableRange(double value)
            => value is >= 0 and <= 1;

        public static bool IsSuitable(double value)
            => value is double.NegativeInfinity || IsInSuitableRange(value: value);
    }
}
