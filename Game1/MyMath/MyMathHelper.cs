using System.Numerics;

namespace Game1.MyMath
{
    public static class MyMathHelper
    {
        public static readonly UDouble pi = (UDouble)Math.PI;
        public static readonly UDouble minPosDouble = (UDouble)1e-6;
        public static readonly decimal minPosDecimal = 1e-6m;

        public static bool AreClose<T>(T value1, T value2) where T : IClose<T>
            => value1.IsCloseTo(other: value2);

        public static bool AreClose(double value1, double value2)
            => IsTiny(value: value1 - value2);

        public static bool AreClose(decimal value1, decimal value2)
            => IsTiny(value: value1 - value2);

        // TODO: consider if this is appropriate
        public static bool AreClose(TimeSpan value1, TimeSpan value2)
            => value1 == value2;

        public static T Min<T>(T value1, T value2) where T : IComparisonOperators<T, T, bool>
            => value1 < value2 ? value1 : value2;

        public static ResAmounts Min(ResAmounts value1, ResAmounts value2)
            => value1.Min(other: value2);

        public static T Max<T>(T value1, T value2) where T : IComparisonOperators<T, T, bool>
            => value1 > value2 ? value1 : value2;

        public static TimeSpan Max(TimeSpan value1, TimeSpan value2)
            => value1 > value2 ? value1 : value2;

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

        public static T Sqrt<T>(T value)
            where T : ISquareRootable<T>
            => value.Sqrt();

        public static UDouble Sqrt(UDouble value)
            => (UDouble)Math.Sqrt(value);

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
            double twoPi = 2 * pi;
            angle = (angle % twoPi + twoPi) % twoPi;
            if (angle > pi)
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
            => new(Cos(rotation), Sin(rotation));

        public static double Rotation(MyVector2 vector)
            => Atan2(vector.Y, vector.X);

        // Coordinate system is such that Y axis points down
        public static MyVector2 Rotate90DegClockwise(MyVector2 vector)
            => new(vector.Y, -vector.X);

        public static bool IsTiny(double value)
            => IsTiny(value: Abs(value));

        public static bool IsTiny(UDouble value)
            => value < minPosDouble;

        public static bool IsTiny(decimal value)
            => Abs(value) < minPosDecimal;

        /// <returns>factor1 * factor2 / divisor rounded using the usual half to even rule https://en.wikipedia.org/wiki/Rounding#Rounding_half_to_even</returns>
        public static ulong MultThenDivideRoundDown(ulong factor1, ulong factor2, ulong divisor)
        {
            UInt128 product = (UInt128)factor1 * factor2;
            ulong quotient = (ulong)(product / divisor),
                remainder = (ulong)(product % divisor);
            return ((long)remainder * 2 - (long)divisor) switch
            {
                < 0 => quotient,
                0 => quotient + (quotient % 2),
                > 0 => quotient + 1
            };
        }
    }
}
