namespace Game1.MyMath
{
    public interface IScalar<T>
    {
        public static abstract Propor Normalize(T value, T start, T stop);

        public static abstract T Interpolate(Propor normalized, T start, T stop);
    }
}
