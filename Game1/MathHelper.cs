namespace Game1
{
    public static class MathHelper
    {
        public const float pi = Microsoft.Xna.Framework.MathHelper.Pi;

        public static T Min<T>(T value1, T value2) where T : IMinable<T>
            => value1.Min(other: value2);

        public static float Min(float value1, float value2)
            => Math.Min(value1, value2);

        public static double Min(double value1, double value2)
            => Math.Min(value1, value2);

        public static int Min(int value1, int value2)
            => Math.Min(value1, value2);

        public static ulong Min(ulong value1, ulong value2)
            => Math.Min(value1, value2);

        public static T Max<T>(T value1, T value2) where T : IMaxable<T>
            => value1.Max(other: value2);

        public static float Max(float value1, float value2)
            => Math.Max(value1, value2);

        public static double Max(double value1, double value2)
            => Math.Max(value1, value2);

        public static int Max(int value1, int value2)
            => Math.Max(value1, value2);

        public static float Abs(float value)
            => Math.Abs(value);

        public static double Abs(double value)
            => Math.Abs(value);

        public static decimal Abs(decimal value)
            => Math.Abs(value);

        public static int Abs(int value)
            => Math.Abs(value);

        public static int Sign(int value)
            => Math.Sign(value);

        public static int Sign(float value)
            => Math.Sign(value);

        public static int Sign(double value)
            => Math.Sign(value);

        public static double Clamp(double value, double min, double max)
            => Math.Clamp(value, min, max);

        public static float Clamp(float value, float min, float max)
            => Math.Clamp(value, min, max);

        public static double Sqrt(double value)
            => Math.Sqrt(value);

        public static double Pow(double @base, double exponent)
            => Math.Pow(@base, exponent);

        public static float WrapAngle(float angle)
            => Microsoft.Xna.Framework.MathHelper.WrapAngle(angle);

        public static double Cos(double rotation)
            => Math.Cos(rotation);

        public static double Sin(double rotation)
            => Math.Sin(rotation);

        public static double Tanh(double rotation)
            => Math.Tanh(rotation);

        public static double Atan2(double y, double x)
            => Math.Atan2(y, x);
    }
}
