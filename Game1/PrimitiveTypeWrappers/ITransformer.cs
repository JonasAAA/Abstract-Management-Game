namespace Game1.PrimitiveTypeWrappers
{
    public interface ITransformer<TParam, TResult>
    {
        public TResult Transform(TParam param);
    }
}
