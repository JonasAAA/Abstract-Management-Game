namespace Game1.Physics
{
    [Serializable]
    public readonly struct AccurVector2
    {
        //private double X
        //    => formalX / C.accurScale;

        //private double Y
        //    => formalY / C.accurScale;

        private readonly long formalX, formalY;

        private AccurVector2(long formalX, long formalY)
        {
            this.formalX = formalX;
            this.formalY = formalY;
        }

        //public static double Distance(AccurVector2 valueA, AccurVector2 valueB)
        //{
        //    var relative = valueA - valueB;
        //    return MathHelper.Sqrt(relative.X * relative.X + relative.Y * relative.Y);
        //}

        // on implicit type conversion https://softwareengineering.stackexchange.com/questions/284359/when-should-i-use-cs-implicit-type-conversion-operator
        public static explicit operator Vector2(AccurVector2 accurVector2)
            => new((float)accurVector2.formalX / C.accurScale, (float)accurVector2.formalY / C.accurScale);

        public static explicit operator AccurVector2(Vector2 vector2)
            => new(Convert.ToInt64(vector2.X * C.accurScale), Convert.ToInt64(vector2.Y * C.accurScale));

        public static AccurVector2 operator -(AccurVector2 a)
            => new(-a.formalX, -a.formalY);

        public static AccurVector2 operator +(AccurVector2 a, AccurVector2 b)
            => new(a.formalX + b.formalX, a.formalY + b.formalY);

        public static AccurVector2 operator -(AccurVector2 a, AccurVector2 b)
            => new(a.formalX - b.formalX, a.formalY - b.formalY);
    }
}
