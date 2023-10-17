using System.Numerics;

namespace Game1.MyMath
{
    public static class MyMathHelper
    {
        public static readonly UDouble pi = (UDouble)Math.PI;
        public static readonly UDouble minPosDouble = (UDouble)1e-6;
        public static readonly decimal minPosDecimal = 1e-5m;

        public static bool AreClose<T>(T value1, T value2) where T : IClose<T>
            => value1.IsCloseTo(other: value2);

        public static bool AreClose(double value1, double value2)
            => IsTiny(value: value1 - value2);

        public static bool AreClose(decimal value1, decimal value2)
            => IsTiny(value: value1 - value2);

        // TODO: consider if this is appropriate
        public static bool AreClose(TimeSpan value1, TimeSpan value2)
            => value1 == value2;

        // T is not IComparisonOperators<T, T, bool> since then if called from a generic method
        // where T happens to be ResAmounts, the incorrect overload is called
        public static T Min<T>(T left, T right) where T : IMin<T>
            => T.Min(left, right);

        public static ulong Min(ulong left, ulong right)
            => left < right ? left : right;

        public static int Min(int left, int right)
            => left < right ? left : right;

        public static decimal Min(decimal left, decimal right)
            => left < right ? left : right;

        public static double Min(double left, double right)
            => left < right ? left : right;

        // T is not IComparisonOperators<T, T, bool> for the same reason as Min function
        public static T Max<T>(T left, T right) where T : IMax<T>
            => T.Max(left, right);

        //public static T Max<T>(T left, T right) where T : IComparisonOperators<T, T, bool>
        //    => left > right ? left : right;

        public static int Max(int left, int right)
            => left > right ? left : right;

        public static double Max(double left, double right)
            => left > right ? left : right;

        public static ulong Max(ulong left, ulong right)
            => left > right ? left : right;

        public static TimeSpan Max(TimeSpan left, TimeSpan right)
            => left > right ? left : right;

        public static UDouble Abs(this double value)
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

        public static ulong Pow(ulong @base, ulong exponent)
        {
            if (exponent is 0)
                return 1;
            if (exponent is 1)
                return @base;
            return Pow(@base: @base * @base, exponent: exponent / 2) * Pow(@base: @base, exponent: exponent % 2);
        }

        public static UDouble Exp(double exponent)
            => (UDouble)Math.Exp(exponent);

        public static double Log(UDouble value)
            => Math.Log(value);

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

        public static double Asin(double value)
            => Math.Asin(value);

        public static double Atan2(SignedLength y, SignedLength x)
            => Atan2(y: y.valueInM, x: x.valueInM);

        public static double Atan2(double y, double x)
            => Math.Atan2(y: y, x: x);

        public static double Tanh(double value)
            => Math.Tanh(value);

        public static Propor Tanh(UDouble value)
            => (Propor)Tanh((double)value);

        public static double Atanh(double value)
            => Math.Atanh(value);

        public static UDouble Atanh(Propor propor)
            => (UDouble)Atanh((double)propor);

        public static Vector2Bare Direction(double rotation)
            => new(Cos(rotation), Sin(rotation));

        public static double Rotation(Vector2Bare vector)
            => Atan2(y: vector.Y, x: vector.X);

        public static double Rotation(MyVector2 vector)
            => Atan2(y: vector.Y, x: vector.X);

        // Coordinate system is such that Y axis points down
        public static MyVector2 Rotate90DegClockwise(MyVector2 vector)
            => new(vector.Y, -vector.X);

        public static bool IsTiny(this double value)
            => IsTiny(value: Abs(value));

        public static bool IsTiny(this UDouble value)
            => value < minPosDouble;

        public static bool IsTiny(decimal value)
            => Abs(value) < minPosDecimal;

        /// <returns>factor1 * factor2 / divisor rounded using the usual half to even rule https://en.wikipedia.org/wiki/Rounding#Rounding_half_to_even</returns>
        public static ulong MultThenDivideRound(ulong factor1, ulong factor2, ulong divisor)
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

        public static ulong DivideThenTakeCeiling(ulong dividend, ulong divisor)
            => (dividend + divisor - 1) / divisor;

        public static ulong Round(UDouble value)
            => (ulong)Math.Round(value);

        public static long Round(double value)
            => (long)Math.Round(value);

        public static long Ceiling(double value)
            => (long)Math.Ceiling(value);

        public static ulong Ceiling(UDouble value)
            => (ulong)Math.Ceiling(value);

        public static long Ceiling(decimal value)
            => (long)Math.Ceiling(value);

        /// <summary>
        /// If part and whole are zero, returns full
        /// </summary>
        public static Propor CreatePropor(ElectricalEnergy part, ElectricalEnergy whole)
        {
            if (part.IsZero && whole.IsZero)
                return Propor.full;
            return Propor.Create(part: part.ValueInJ, whole: whole.ValueInJ)!.Value;
        }

        /// <summary>
        /// Meaning is equivalent to ((Real)numeratorA / denominatorA).CompareTo((Real)numeratorB / denominatorB)
        /// if infinite precision Real type existed
        /// </summary>
        public static int CompareFractions(ulong numeratorA, ulong denominatorA, ulong numeratorB, ulong denominatorB)
            => ((UInt128)numeratorA * denominatorB).CompareTo((UInt128)numeratorB * denominatorA);
    }
}
