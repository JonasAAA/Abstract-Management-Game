using System;

namespace Game1.Physics
{
    public readonly struct AccurNum
    {
        private readonly long num;

        private AccurNum(long num)
            => this.num = num;

        public static explicit operator float(AccurNum a)
            => (float)a.num / C.accurScale;

        public static explicit operator AccurNum(float a)
            => new(Convert.ToInt64(a * C.accurScale));

        public static AccurNum operator -(AccurNum a)
            => new(-a.num);

        public static AccurNum operator +(AccurNum a, AccurNum b)
            => new(a.num + b.num);

        public static AccurNum operator -(AccurNum a, AccurNum b)
            => new(a.num - b.num);
    }
}
