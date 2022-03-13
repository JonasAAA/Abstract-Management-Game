namespace Game1.PrimitiveTypeWrappers
{
    public interface ITransform<TValue, TResult>
    {
        public TResult Transform(TValue value);
    }

    public interface ITransform<TParam, TValue, TResult>
    {
        public TResult Transform(TParam param, TValue value);
    }
}
