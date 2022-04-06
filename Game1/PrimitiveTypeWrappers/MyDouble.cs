//namespace Game1.PrimitiveTypeWrappers
//{
//    public readonly struct double : IMinable<double>, IMaxable<double>
//    {
//        private readonly double value;

//        private double(double value)
//            => this.value = value;

//        public static bool operator <(double value1, double value2)
//            => value1.value < value2.value;

//        public static bool operator >(double value1, double value2)
//            => value1.value > value2.value;

//        public static double operator +(double value)
//            => value;

//        public static double operator -(double value)
//            => new(value: -value.value);

//        public static double operator +(double value1, double value2)
//            => new(value: value1.value + value2.value);

//        public static double operator -(double value1, double value2)
//            => new(value: value1.value - value2.value);

//        public static implicit operator double(double value)
//            => new(value: value);

//        double IMaxable<double>.Max(double other)
//            => this < other ? other : this;

//        double IMinable<double>.Min(double other)
//            => this < other ? this : other;
//    }
//}
