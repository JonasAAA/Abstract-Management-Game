namespace Game1.MyMath
{
    public static class MyMathHelper
    {
        public static readonly UDouble pi = (UDouble)Math.PI;
        private static readonly UDouble minPosDouble = (UDouble)1e-6;
        private static readonly decimal minPosDecimal = 1e-6m;

        public static bool AreClose<T>(T value1, T value2) where T : IClose<T>
            => value1.IsCloseTo(other: value2);

        public static bool AreClose(double value1, double value2)
            => IsTiny(value: value1 - value2);

        public static bool AreClose(decimal value1, decimal value2)
            => IsTiny(value: value1 - value2);

        public static T Min<T>(T value1, T value2) where T : IMinable<T>
            => value1.Min(other: value2);

        public static double Min(double value1, double value2)
            => Math.Min(value1, value2);

        public static int Min(int value1, int value2)
            => Math.Min(value1, value2);

        public static ulong Min(ulong value1, ulong value2)
            => Math.Min(value1, value2);

        public static T Max<T>(T value1, T value2) where T : IMaxable<T>
            => value1.Max(other: value2);

        public static double Max(double value1, double value2)
            => Math.Max(value1, value2);

        public static int Max(int value1, int value2)
            => Math.Max(value1, value2);

        public static UDouble Abs(double value)
            => (UDouble)Math.Abs(value);

        public static decimal Abs(decimal value)
            => Math.Abs(value);

        public static int Abs(int value)
            => Math.Abs(value);

        public static int Sign(int value)
            => Math.Sign(value);

        public static int Sign(double value)
            => Math.Sign(value);

        public static double Clamp(double value, double min, double max)
            => Math.Clamp(value, min, max);

        public static UDouble Square(double value)
            => (UDouble)(value * value);

        public static UDouble Sqrt(UDouble value)
            => (UDouble)Math.Sqrt((double)value);

        public static TBase Pow<TBase, TExponent>(TBase @base, TExponent exponent)
            where TBase : IExponentiable<TExponent, TBase>
            => @base.Pow(exponent: exponent);

        public static double Pow(double @base, double exponent)
            => Math.Pow(@base, exponent);

        /// <summary>
        /// Returns equivalent angle between -pi and pi
        /// </summary>
        public static double WrapAngle(double angle)
        {
            double twoPi = 2 * (double)pi;
            angle = (angle % twoPi + twoPi) % twoPi;
            if (angle > (double)pi)
                angle -= twoPi;
            return angle;
        }

        public static double Cos(double rotation)
            => Math.Cos(rotation);

        public static double Sin(double rotation)
            => Math.Sin(rotation);

        public static double Atan2(double y, double x)
            => Math.Atan2(y, x);

        public static double Tanh(double value)
            => Math.Tanh(value);

        public static Propor Tanh(UDouble value)
            => (Propor)Tanh((double)value);

        public static double Atanh(double value)
            => Math.Atanh(value);

        public static UDouble Atanh(Propor propor)
            => (UDouble)Atanh((double)propor);

        public static MyVector2 Direction(double rotation)
            => new((double)Cos(rotation), (double)Sin(rotation));

        public static double Rotation(MyVector2 vector)
            => (double)Atan2(vector.Y, vector.X);

        /// <summary>
        /// 90 degrees to the left
        /// </summary>
        public static MyVector2 OrthDir(MyVector2 direction)
            => new(direction.Y, -direction.X);

        public static bool IsTiny(double value)
            => IsTiny(value: Abs(value));

        public static bool IsTiny(UDouble value)
            => value < minPosDouble;

        public static bool IsTiny(decimal value)
            => Abs(value) < minPosDecimal;
    }
}
